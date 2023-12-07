using System;
using System.IO;
using FontStashSharp;
using MariEngine.Components;
using MariEngine.Light;
using MariEngine.Services;
using MariEngine.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MariEngine.Logging;
using MariEngine.Rendering;

namespace MariEngine.Tiles;

public class TilemapRenderer(GraphicsDevice graphicsDevice, Camera camera) : Renderer(graphicsDevice, camera)
{
    private Tilemap tilemap;
    private LightMap lightMap;

    protected override void OnAttach()
    {
        tilemap = GetComponent<Tilemap>();
        lightMap = GetComponent<LightMap>();
    }

    protected override void Render(SpriteBatch spriteBatch)
    {
        var cullingBounds = GetCullingBounds();
        if (cullingBounds is null) return;
        
        spriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: camera.TransformMatrix);

        for (float y = cullingBounds.Value.TopLeft.Y; y < cullingBounds.Value.BottomRight.Y + 1; y++)
        {
            for (float x = cullingBounds.Value.TopLeft.X; x < cullingBounds.Value.BottomRight.X + 1; x++)
            {
                Coord coord = new(x, y);
                if (!tilemap.IsInBounds(coord)) continue;
                RenderTile(spriteBatch, tilemap.CoordToWorldPoint(coord), tilemap.GetTop(coord), lightMap.GetRenderedLight(coord));
            }
        }
        
        spriteBatch.End();
        
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: camera.TransformMatrix);

        foreach (TileEntity entity in tilemap.TileEntities)
        {
            if (!cullingBounds.Value.PointInside((Vector2)entity.Position)) continue;
            
            RenderTile(spriteBatch, tilemap.Vector2ToWorldPoint(entity.SmoothedPosition), entity.Tile, lightMap.GetRenderedLight(entity.Position));
        }

        spriteBatch.End();
    }

    internal Bounds? GetCullingBounds()
    {
        var cullingBoundsTest = GetWorldCullingBounds();
        if (cullingBoundsTest is null) return null;
        
        Bounds cullingBounds = Bounds.MakeCorners((Vector2)tilemap.WorldPointToCoord(cullingBoundsTest.Value.TopLeft),
            (Vector2)tilemap.WorldPointToCoord(cullingBoundsTest.Value.BottomRight));
        return cullingBounds;
    }

    internal Bounds? GetWorldCullingBounds()
    {
        Bounds cameraBounds = camera.ViewingWindow;
        Bounds tilemapBounds = new(tilemap.CoordToWorldPoint(Coord.Zero), new Vector2(tilemap.Width, tilemap.Height));
        Bounds? overlap = Bounds.Overlap(cameraBounds, tilemapBounds);
        return overlap;
    }

    protected override Vector2 CalculateCenterOffset() => tilemap.CalculateCenterOffset();

    private void RenderTile(SpriteBatch spriteBatch, Vector2 pos, Tile tile, Color tint)
    {
        // TODO: Add scaling support back (it was nuked when switching over to the tile atlas)
        
        // TODO: Optimize this to use GPU instancing (or whatever it's called, drawing primitives with setup vertex/index buffers)
        // 1 or 2 draw calls (one for background, one for foreground)
        // https://badecho.com/index.php/2022/08/04/drawing-tiles/
        if (tile is not null)
            ServiceRegistry.Get<TileAtlas>().DrawTile(spriteBatch, pos * camera.TileSize, tile.Id, tint);
    }
}