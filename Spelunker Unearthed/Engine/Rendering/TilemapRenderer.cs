using System.IO;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpelunkerUnearthed.Engine.Components;
using SpelunkerUnearthed.Engine.Light;
using SpelunkerUnearthed.Engine.Services;
using SpelunkerUnearthed.Engine.Tiles;

namespace SpelunkerUnearthed.Engine.Rendering;

public class TilemapRenderer : Renderer
{
    private Tilemap tilemap;
    private LightMap lightMap;
    
    public TilemapRenderer(GraphicsDevice graphicsDevice)
    {
        
    }

    public override void OnAttach()
    {
        tilemap = GetComponent<Tilemap>();
        lightMap = GetComponent<LightMap>();
    }

    public override void Render(SpriteBatch spriteBatch, Camera camera)
    {
        var transform = GetComponent<Transform>();
        
        lightMap.RenderLightMap();
        
        spriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: camera.WorldToScreenMatrix);
        
        foreach (Coord coord in tilemap.Coords)
        {
            RenderTile(spriteBatch, camera, CalculatePosition(coord), tilemap[coord], lightMap.GetRenderedLight(coord));
        }
        
        spriteBatch.End();
        
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: camera.WorldToScreenMatrix);

        foreach (TileEntity entity in tilemap.TileEntities)
        {
            RenderTile(spriteBatch, camera, CalculatePosition(entity.Position), entity.Tile, lightMap.GetRenderedLight(entity.Position));
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

    private void RenderTile(SpriteBatch spriteBatch, Camera camera, Vector2 pos, Tile tile, Color tint)
    {
        // TODO: Add scaling support back (it was nuked when switching over to the tile atlas)
        
        // TODO: Optimize this to use GPU instancing (or whatever it's called, drawing primitives with setup vertex/index buffers)
        // 1 or 2 draw calls (one for background, one for foreground)
        // https://badecho.com/index.php/2022/08/04/drawing-tiles/
        ServiceRegistry.Get<TileAtlas>().DrawTile(spriteBatch, pos, tile.Id, tint);
    }
}