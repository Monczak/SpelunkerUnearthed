using System.IO;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpelunkerUnearthed.Engine.Components;
using SpelunkerUnearthed.Engine.Services;
using SpelunkerUnearthed.Engine.Tiles;

namespace SpelunkerUnearthed.Engine.Rendering;

public class TilemapRenderer : Renderer
{
    private Tilemap tilemap;
    
    public TilemapRenderer(GraphicsDevice graphicsDevice)
    {
        
    }

    public override void OnAttach()
    {
        tilemap = GetComponent<Tilemap>();
    }

    public override void Render(SpriteBatch spriteBatch, Camera camera)
    {
        var transform = GetComponent<Transform>();
        
        spriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: camera.WorldToScreenMatrix);
        
        for (int y = 0; y < tilemap.MapHeight; y++)
        {
            for (int x = 0; x < tilemap.MapWidth; x++)
            {
                RenderTile(spriteBatch, camera, CalculatePosition(new Coord(x, y)), tilemap[x, y]);
            }
        }
        
        spriteBatch.End();
        
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: camera.WorldToScreenMatrix);

        foreach (TileEntity entity in tilemap.TileEntities)
        {
            RenderTile(spriteBatch, camera, CalculatePosition(entity.Position), entity.Tile);
        }

        spriteBatch.End();
        
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
        // TODO: Add scaling support back (it was nuked when switching over to the tile atlas)
        
        // TODO: Optimize this to use GPU instancing (or whatever it's called, drawing primitives with setup vertex/index buffers)
        // 1 or 2 draw calls (one for background, one for foreground)
        // https://badecho.com/index.php/2022/08/04/drawing-tiles/
        ServiceRegistry.Get<TileAtlas>().DrawTile(spriteBatch, pos, tile.Id);
    }
}