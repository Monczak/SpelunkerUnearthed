using System.IO;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpelunkerUnearthed.Engine.Tiles;

namespace SpelunkerUnearthed.Engine.Rendering;

public class TileRenderer : Renderer
{
    private Tilemap tilemap;

    public int TileSize { get; } = 24;

    private FontSystem fontSystem;
    private SpriteFontBase font;

    private readonly Texture2D backgroundTexture;
    
    public TileRenderer(GraphicsDevice graphicsDevice, Tilemap tilemap)
    {
        this.tilemap = tilemap;
        fontSystem = new FontSystem();

        backgroundTexture = new Texture2D(graphicsDevice, 1, 1);
        backgroundTexture.SetData(new[] { Color.White });
        
        AddFont("Hack-Regular");
        AddFont("Monospace");
    }

    public void AddFont(string fontName)
    {
        fontSystem.AddFont(File.ReadAllBytes($"Content/Fonts/{fontName}.ttf"));
    }

    public Vector2 CalculateOffset(char character)
    {
        Vector2 charSize = font.MeasureString(character.ToString());
        return new Vector2(
            TileSize / 2f - charSize.X / 2f,
            TileSize / 2f - charSize.Y / 2f
        );
    }
    
    public override void Render(SpriteBatch spriteBatch)
    {
        font = fontSystem.GetFont(TileSize);
        
        for (int y = 0; y < tilemap.MapWidth; y++)
        {
            for (int x = 0; x < tilemap.MapWidth; x++)
            {
                RenderTile(spriteBatch, new Vector2(x * TileSize, y * TileSize), tilemap[x, y]);
            }
        }
    }

    private void RenderTile(SpriteBatch spriteBatch, Vector2 pos, Tile tile)
    {
        spriteBatch.Draw(backgroundTexture, new Rectangle((int)pos.X, (int)pos.Y, TileSize, TileSize), tile.BackgroundColor);
        spriteBatch.DrawString(font, tile.Character.ToString(), pos + CalculateOffset(tile.Character), tile.ForegroundColor);
    }
}