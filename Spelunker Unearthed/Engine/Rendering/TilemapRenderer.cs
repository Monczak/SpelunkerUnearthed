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
    private Transform transform;
    
    public TilemapRenderer(GraphicsDevice graphicsDevice, Camera camera) : base(camera)
    {
        
    }

    public override void OnAttach()
    {
        tilemap = GetComponent<Tilemap>();
        lightMap = GetComponent<LightMap>();
        transform = GetComponent<Transform>();
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        lightMap.RenderLightMap();
        
        spriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: camera.TransformMatrix);
        
        foreach (Coord coord in tilemap.Coords)
        {
            RenderTile(spriteBatch, CoordToWorldPoint(coord), tilemap[coord], lightMap.GetRenderedLight(coord));
        }
        
        spriteBatch.End();
        
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: camera.TransformMatrix);

        foreach (TileEntity entity in tilemap.TileEntities)
        {
            RenderTile(spriteBatch, CoordToWorldPoint(entity.Position), entity.Tile, lightMap.GetRenderedLight(entity.Position));
        }

        spriteBatch.End();
    }
    
    public Vector2 CoordToWorldPoint(Coord coord)
    {
        return (Vector2)coord + transform.Position + CalculateCenterOffset();
    }

    public Coord WorldPointToCoord(Vector2 point)
    {
        return (Coord)(point - CalculateCenterOffset() - transform.Position);
    }

    protected override Vector2 CalculateCenterOffset()
    {
        return -new Vector2(tilemap.MapWidth / 2f, tilemap.MapHeight / 2f);
    }

    private void RenderTile(SpriteBatch spriteBatch, Vector2 pos, Tile tile, Color tint)
    {
        // TODO: Add scaling support back (it was nuked when switching over to the tile atlas)
        
        // TODO: Optimize this to use GPU instancing (or whatever it's called, drawing primitives with setup vertex/index buffers)
        // 1 or 2 draw calls (one for background, one for foreground)
        // https://badecho.com/index.php/2022/08/04/drawing-tiles/
        ServiceRegistry.Get<TileAtlas>().DrawTile(spriteBatch, pos * camera.TileSize, tile.Id, tint);
    }
}