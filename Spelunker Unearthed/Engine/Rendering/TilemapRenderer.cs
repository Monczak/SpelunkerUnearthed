using System.IO;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpelunkerUnearthed.Engine.Components;
using SpelunkerUnearthed.Engine.Tiles;

namespace SpelunkerUnearthed.Engine.Rendering;

public class TilemapRenderer : Renderer
{
    private Tilemap tilemap;

    private FontSystem fontSystem;
    private SpriteFontBase font;

    private readonly Texture2D backgroundTexture;
    
    public TilemapRenderer(GraphicsDevice graphicsDevice, Tilemap tilemap)
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

    public Vector2 CalculateTextOffset(char character, int tileSize)
    {
        Vector2 charSize = font.MeasureString(character.ToString());
        return new Vector2(
            tileSize / 2f - charSize.X / 2f,
            tileSize / 2f - charSize.Y / 2f
        );
    }

    public override void Render(SpriteBatch spriteBatch, Camera camera)
    {
        var transform = GetComponent<Transform>();

        font = fontSystem.GetFont(camera.TileSize);
        
        for (int y = 0; y < tilemap.MapHeight; y++)
        {
            for (int x = 0; x < tilemap.MapWidth; x++)
            {
                RenderTile(spriteBatch, camera, CalculatePosition(new Coord(x, y)), tilemap[x, y]);
            }
        }

        foreach (TileEntity entity in tilemap.TileEntities)
        {
            RenderTile(spriteBatch, camera, CalculatePosition(entity.Position), entity.Tile);
        }

        return;

        Vector2 CalculatePosition(Coord coord)
        {
            return ((Vector2)coord + transform.Position) * camera.TileSize + CalculateCenterOffset(camera);
        }
    }

    protected override Vector2 CalculateCenterOffset(Camera camera)
    {
        return -new Vector2(camera.TileSize * tilemap.MapWidth / 2f, camera.TileSize * tilemap.MapHeight / 2f);
    }

    private void RenderTile(SpriteBatch spriteBatch, Camera camera, Vector2 pos, Tile tile)
    {
        spriteBatch.Draw(backgroundTexture, new Rectangle((int)pos.X, (int)pos.Y, camera.TileSize, camera.TileSize), tile.BackgroundColor);
        spriteBatch.DrawString(font, tile.Character.ToString(), pos + CalculateTextOffset(tile.Character, camera.TileSize), tile.ForegroundColor);
    }
}