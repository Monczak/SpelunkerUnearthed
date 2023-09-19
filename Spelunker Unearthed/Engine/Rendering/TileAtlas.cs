using System;
using System.Collections.Generic;
using System.IO;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpelunkerUnearthed.Engine.Logging;
using SpelunkerUnearthed.Engine.Services;
using SpelunkerUnearthed.Engine.Tiles;

namespace SpelunkerUnearthed.Engine.Rendering;

public class TileAtlas : Service
{
    private RenderTarget2D backgroundRenderTarget, foregroundRenderTarget;
    
    private FontSystem fontSystem;
    private SpriteFontBase font;

    private GraphicsDevice graphicsDevice;
    private SpriteBatch spriteBatch;
    private int tileSize;

    private Coord atlasSize;

    private Dictionary<string, Coord> tileAtlasCoords;

    private readonly Texture2D backgroundTexture;
    
    public TileAtlas(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, int tileSize)
    {
        this.graphicsDevice = graphicsDevice;
        this.spriteBatch = spriteBatch;
        this.tileSize = tileSize;
        
        backgroundTexture = new Texture2D(graphicsDevice, 1, 1);
        backgroundTexture.SetData(new[] { Color.White });
        
        fontSystem = new FontSystem();
        
        AddFont("Hack-Regular");
        AddFont("Monospace");
    }
    
    public void AddFont(string fontName)
    {
        // TODO: Don't use hardcoded paths
        fontSystem.AddFont(File.ReadAllBytes($"Content/Fonts/{fontName}.ttf"));
    }
    
    public Vector2 CalculateTextOffset(char character)
    {
        Vector2 charSize = font.MeasureString(character.ToString());
        return new Vector2(
            tileSize / 2f - charSize.X / 2f,
            tileSize / 2f - charSize.Y / 2f
        );
    }

    public void CreateAtlas(Dictionary<string, Tile> tiles)
    {
        InitializeRenderTargets(tiles);
        ReserveCoords(tiles);
        DrawBackgrounds(tiles.Values);
        DrawForegrounds(tiles.Values);
        
        Logger.Log($"Created 2x {atlasSize.X}x{atlasSize.Y} ({atlasSize.X * tileSize}x{atlasSize.Y * tileSize}) tile atlas");
    }

    private void InitializeRenderTargets(Dictionary<string, Tile> tiles)
    {
        atlasSize = CalculateAtlasSize(tiles.Count);

        backgroundRenderTarget = new RenderTarget2D(
            graphicsDevice,
            atlasSize.X * tileSize,
            atlasSize.Y * tileSize,
            false,
            graphicsDevice.PresentationParameters.BackBufferFormat,
            graphicsDevice.PresentationParameters.DepthStencilFormat
        );

        foregroundRenderTarget = new RenderTarget2D(
            graphicsDevice,
            atlasSize.X * tileSize,
            atlasSize.Y * tileSize,
            false,
            graphicsDevice.PresentationParameters.BackBufferFormat,
            graphicsDevice.PresentationParameters.DepthStencilFormat
        );
    }

    private void ReserveCoords(Dictionary<string, Tile> tiles)
    {
        tileAtlasCoords = new Dictionary<string, Coord>();
        List<string> ids = new List<string>(tiles.Keys);
        for (int y = 0; y < atlasSize.Y; y++)
        {
            for (int x = 0; x < atlasSize.X; x++)
            {
                int i = x + y * atlasSize.X;
                tileAtlasCoords[ids[i]] = new Coord(x, y);
            }
        }
    }

    private void DrawBackgrounds(IEnumerable<Tile> tiles)
    {
        graphicsDevice.SetRenderTarget(backgroundRenderTarget);
        graphicsDevice.Clear(Color.Black);
        
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
        
        foreach (Tile tile in tiles)
        {
            Coord coord = tileAtlasCoords[tile.Id];
            spriteBatch.Draw(backgroundTexture, new Rectangle(coord.X * tileSize, coord.Y * tileSize, tileSize, tileSize), tile.BackgroundColor);
        }

        spriteBatch.End();
        
        graphicsDevice.SetRenderTarget(null);
    }

    private void DrawForegrounds(IEnumerable<Tile> tiles)
    {
        font = fontSystem.GetFont(tileSize);
        
        graphicsDevice.SetRenderTarget(foregroundRenderTarget);
        graphicsDevice.Clear(Color.Transparent);
        
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
        
        foreach (Tile tile in tiles)
        {
            Coord coord = tileAtlasCoords[tile.Id];
            spriteBatch.DrawString(font, tile.Character.ToString(), (Vector2)coord * tileSize + CalculateTextOffset(tile.Character), tile.ForegroundColor);
        }

        spriteBatch.End();
        
        graphicsDevice.SetRenderTarget(null);
    }

    private Coord CalculateAtlasSize(int tileCount)
    {
        int atlasSizeX = (int)Math.Sqrt(tileCount);
        int atlasSizeY = (int)Math.Ceiling((float)tileCount / atlasSizeX);

        return new Coord(atlasSizeX, atlasSizeY);
    }

    public void DrawTile(SpriteBatch localSpriteBatch, Vector2 pos, string tileId, Color tint)
    {
        Coord atlasCoord = tileAtlasCoords[tileId];
        localSpriteBatch.Draw(backgroundRenderTarget, pos, new Rectangle(atlasCoord.X * tileSize, atlasCoord.Y * tileSize, tileSize, tileSize), tint);
        localSpriteBatch.Draw(foregroundRenderTarget, pos, new Rectangle(atlasCoord.X * tileSize, atlasCoord.Y * tileSize, tileSize, tileSize), tint);
    }
}