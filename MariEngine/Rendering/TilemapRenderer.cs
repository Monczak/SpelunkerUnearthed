using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FontStashSharp;
using MariEngine.Components;
using MariEngine.Light;
using MariEngine.Services;
using MariEngine.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MariEngine.Logging;

namespace MariEngine.Rendering;

public class TilemapRenderer(GraphicsDevice graphicsDevice, Camera camera) : Renderer(graphicsDevice, camera)
{
    private Tilemap tilemap;
    private LightMap lightMap;
    
    protected readonly List<TilemapRendererEffect> Effects = [];
    protected readonly Dictionary<Type, TilemapRendererEffect> EffectsByType = new();

    public void AddEffect(TilemapRendererEffect effect)
    {
        Effects.Add(effect);
        EffectsByType.Add(effect.GetType(), effect);
        Effects.Sort((effect1, effect2) => effect1.Priority - effect2.Priority);
    }

    public void RemoveEffect(TilemapRendererEffect effect)
    {
        EffectsByType.Remove(effect.GetType());
        Effects.Remove(effect);
    }

    public T GetEffect<T>() where T : TilemapRendererEffect
    {
        return (T)EffectsByType[typeof(T)];
    }

    protected internal override void Initialize()
    {
        tilemap = GetComponent<Tilemap>();
        lightMap = GetComponent<LightMap>();
        
        Effects.Add(new LightMapEffect(lightMap) { Priority = -1000 });
    }

    protected override void Render(SpriteBatch spriteBatch, GameTime gameTime)
    {
        var cullingBounds = GetCullingBounds();
        if (cullingBounds is null) return;
        
        spriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: Camera.TransformMatrix);

        for (float y = cullingBounds.Value.TopLeft.Y; y < cullingBounds.Value.BottomRight.Y + 1; y++)
        {
            for (float x = cullingBounds.Value.TopLeft.X; x < cullingBounds.Value.BottomRight.X + 1; x++)
            {
                Coord coord = new(x, y);
                if (!tilemap.IsInBounds(coord)) continue;

                var tile = tilemap.GetTop(coord, out var layer);
                RenderTile(spriteBatch, coord, tile, layer, gameTime);
            }
        }
        
        spriteBatch.End();
        
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: Camera.TransformMatrix);

        Parallel.ForEach(tilemap.TileEntities, entity =>
        {
            var renderer = entity.GetComponent<TileEntityRenderer>();

            if (renderer is null) return;
            if (Bounds.Overlap(cullingBounds.Value, renderer.GetCullingBounds()) is null) return;

            renderer.Render(spriteBatch, camera, graphicsDevice, Effects, gameTime);
        });

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
        Bounds cameraBounds = Camera.ViewingWindow;
        Bounds tilemapBounds = new(tilemap.CoordToWorldPoint(Coord.Zero), new Vector2(tilemap.Width, tilemap.Height));
        Bounds? overlap = Bounds.Overlap(cameraBounds, tilemapBounds);
        return overlap;
    }

    protected override Vector2 CalculateCenterOffset() => tilemap.CalculateCenterOffset();

    private void RenderTile(SpriteBatch spriteBatch, Coord pos, Tile tile, TilemapLayer layer, GameTime gameTime)
    {
        // TODO: Add scaling support back (it was nuked when switching over to the tile atlas)
        
        // TODO: Optimize this to use GPU instancing (or whatever it's called, drawing primitives with setup vertex/index buffers)
        // 1 or 2 draw calls (one for background, one for foreground)
        // https://badecho.com/index.php/2022/08/04/drawing-tiles/
        if (tile is null) return;
        
        var foregroundTint = tile.ForegroundColor;
        var backgroundTint = tile.BackgroundColor;

        foreach (var effect in Effects)
        {
            if ((layer & effect.LayerMask) == 0) continue;
            
            foregroundTint = effect.Apply(foregroundTint, pos, gameTime);
            backgroundTint = effect.Apply(backgroundTint, pos, gameTime);
        }
            
        ServiceRegistry.Get<TileAtlas>().DrawTile(spriteBatch,  tilemap.CoordToWorldPoint(pos) * Camera.TileSize, tile.Id, foregroundTint, backgroundTint);
    }
}