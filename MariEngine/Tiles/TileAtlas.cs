using System;
using System.Collections.Generic;
using System.IO;
using FontStashSharp;
using MariEngine.Events;
using MariEngine.Logging;
using MariEngine.Services;
using MariEngine.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MariEngine.Tiles;

public class TileAtlas : Service
{
    private RenderTarget2D backgroundRenderTarget, foregroundRenderTarget;

    private SpriteFontBase worldFont;
    private SpriteFontBase uiFont;

    private GraphicsDevice graphicsDevice;
    private SpriteBatch spriteBatch;

    private IDictionary<string, Tile> tileDict;
    
    public int TileSize { get; private set; }
    private int tileMarginSize = 1;

    private int TileSizeWithMargin => TileSize + tileMarginSize * 2;

    private Coord atlasSize;

    private Dictionary<string, Coord> tileAtlasCoords;

    private readonly Texture2D backgroundTexture;
    
    public TileAtlas(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, int tileSize)
    {
        this.graphicsDevice = graphicsDevice;
        this.spriteBatch = spriteBatch;
        TileSize = tileSize;
        
        backgroundTexture = new Texture2D(graphicsDevice, 1, 1);
        backgroundTexture.SetData([Color.White]);
    }

    private Vector2 CalculateTextOffset(SpriteFontBase font, char character)
    {
        Vector2 charSize = font.MeasureString(character.ToString());
        return new Vector2(
            TileSize / 2f - charSize.X / 2f,
            TileSize / 2f - charSize.Y / 2f
        );
    }

    public void SetTiles(IDictionary<string, Tile> tiles)
    {
        tileDict = tiles;
    }

    public void Resize(int newTileSize)
    {
        TileSize = newTileSize;
        ServiceRegistry.Get<EventManager>().Notify("TileAtlasResized", newTileSize);
        CreateAtlas();
    }

    public void CreateAtlas()
    {
        InitializeRenderTargets(tileDict);
        ReserveCoords(tileDict);
        DrawBackgrounds(tileDict.Values);
        DrawForegrounds(tileDict.Values);
        
        Logger.Log($"Created 2x {atlasSize.X}x{atlasSize.Y} ({atlasSize.X * TileSize}x{atlasSize.Y * TileSize}) tile atlas");
    }

    private void InitializeRenderTargets(IDictionary<string, Tile> tiles)
    {
        atlasSize = CalculateAtlasSize(tiles.Count);

        backgroundRenderTarget = new RenderTarget2D(
            graphicsDevice,
            atlasSize.X * TileSizeWithMargin,
            atlasSize.Y * TileSizeWithMargin,
            false,
            graphicsDevice.PresentationParameters.BackBufferFormat,
            graphicsDevice.PresentationParameters.DepthStencilFormat
        );

        foregroundRenderTarget = new RenderTarget2D(
            graphicsDevice,
            atlasSize.X * TileSizeWithMargin,
            atlasSize.Y * TileSizeWithMargin,
            false,
            graphicsDevice.PresentationParameters.BackBufferFormat,
            graphicsDevice.PresentationParameters.DepthStencilFormat
        );
    }

    private void ReserveCoords(IDictionary<string, Tile> tiles)
    {
        tileAtlasCoords = new Dictionary<string, Coord>();
        List<string> ids = [..tiles.Keys];
        for (int y = 0; y < atlasSize.Y; y++)
        {
            for (int x = 0; x < atlasSize.X; x++)
            {
                int i = x + y * atlasSize.X;
                if (i >= tiles.Count) return;
                
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
            spriteBatch.Draw(backgroundTexture, new Rectangle(coord.X * TileSizeWithMargin, coord.Y * TileSizeWithMargin, TileSizeWithMargin, TileSizeWithMargin), Color.White);
        }

        spriteBatch.End();
        
        graphicsDevice.SetRenderTarget(null);
    }

    private void DrawForegrounds(IEnumerable<Tile> tiles)
    {
        worldFont = ServiceRegistry.Get<FontProvider>().GetFont("Tiles", TileSize);
        uiFont = ServiceRegistry.Get<FontProvider>().GetFont("UiFont", TileSize);
        
        graphicsDevice.SetRenderTarget(foregroundRenderTarget);
        graphicsDevice.Clear(Color.Transparent);
        
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
        
        foreach (Tile tile in tiles)
        {
            Coord coord = tileAtlasCoords[tile.Id];
            var font = tile.Type is TileType.Ui ? uiFont : worldFont;
            spriteBatch.DrawString(font, tile.Character.ToString(), (Vector2)coord * TileSizeWithMargin + Vector2.One * tileMarginSize + CalculateTextOffset(font, tile.Character), Color.White);
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

    public void DrawTile(SpriteBatch localSpriteBatch, Vector2 pos, string tileId, Color foregroundTint, Color backgroundTint)
    {
        var atlasCoord = tileAtlasCoords[tileId];

        localSpriteBatch.Draw(backgroundRenderTarget, pos, new Rectangle(atlasCoord.X * TileSizeWithMargin + tileMarginSize, atlasCoord.Y * TileSizeWithMargin + tileMarginSize, TileSize, TileSize), backgroundTint);
        localSpriteBatch.Draw(foregroundRenderTarget, pos, new Rectangle(atlasCoord.X * TileSizeWithMargin + tileMarginSize, atlasCoord.Y * TileSizeWithMargin + tileMarginSize, TileSize, TileSize), foregroundTint);
    }
}