using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using MariEngine.Components;
using MariEngine.Logging;
using MariEngine.Services;
using MariEngine.Tiles;
using Microsoft.Xna.Framework;

namespace MariEngine.Light;

public class LightMap : Component
{
    private Tilemap tilemap;
    public Color AmbientLight { get; set; }

    private HashSet<LightEmitter> lightEmitters;
    private Dictionary<Tile, Coord> lightEmittingTiles;

    private Vector3[,] map;

    public int RenderThreads { get; set; } = 4;

    public override void OnAttach()
    {
        tilemap = GetComponent<Tilemap>();
        lightEmitters = new HashSet<LightEmitter>();
        lightEmittingTiles = new Dictionary<Tile, Coord>();

        map = new Vector3[tilemap.MapWidth, tilemap.MapHeight];
    }

    public void AddEmittingTile(Tile tile, Coord position)
    {
        lightEmittingTiles.Add(tile, position);
    }

    public void RemoveEmittingTile(Tile tile)
    {
        if (lightEmittingTiles.ContainsKey(tile))
            lightEmittingTiles.Remove(tile);
    }

    public void AddEmitter(LightEmitter emitter)
    {
        lightEmitters.Add(emitter);
    }

    public void RemoveEmitter(LightEmitter emitter)
    {
        if (lightEmitters.Contains(emitter))
            lightEmitters.Remove(emitter);
    }

    public void RenderLightMap(Bounds cullingBounds)
    {
        for (int i = 0; i < tilemap.MapHeight; i++)
        {
            for (int j = 0; j < tilemap.MapWidth; j++)
            {
                map[j, i] = AmbientLight.ToVector3();
            }
        }

        foreach (LightEmitter emitter in lightEmitters)
        {
            Bounds? lightBounds = emitter.Light.GetBounds(emitter.OwnerEntity.Position);
            Bounds? renderBounds;
            if (lightBounds is null)
                renderBounds = cullingBounds;
            else
                renderBounds = Bounds.Overlap(cullingBounds, lightBounds.Value);
            if (renderBounds is null) continue;
            
            RenderLight(emitter.Light, emitter.OwnerEntity.Position, renderBounds.Value);
        }
        
        foreach (var pair in lightEmittingTiles)
        {
            Bounds? lightBounds = pair.Key.LightSource.GetBounds(pair.Value);
            Bounds? renderBounds;
            if (lightBounds is null)
                renderBounds = cullingBounds;
            else
                renderBounds = Bounds.Overlap(cullingBounds, lightBounds.Value);
            if (renderBounds is null) continue;
            
            RenderLight(pair.Key.LightSource, pair.Value, renderBounds.Value);
        }
    }

    private void RenderLight(LightSource source, Coord position, Bounds renderBounds)
    {
        for (float x = renderBounds.TopLeft.X; x < renderBounds.BottomRight.X + 1; x++)
        {
            for (float y = renderBounds.TopLeft.Y; y < renderBounds.BottomRight.Y + 1; y++)
            {
                Coord coord = (Coord)new Vector2(x, y);
                if (!tilemap.IsInBounds(coord)) continue;
                map[coord.X, coord.Y] += source.GetLight(position, coord).ToVector3();
            }
        }
    }

    public Color GetRenderedLight(Coord position)
    {
        Vector3 color = map[position.X, position.Y];
        float max = MathF.Max(color.X, MathF.Max(color.Y, color.Z));
        if (max > 1)
            color /= max;
        return new Color(color);
    }
}