using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using MariEngine.Components;
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

    private Color[,] map;

    public int RenderThreads { get; set; } = 4;

    public override void OnAttach()
    {
        tilemap = GetComponent<Tilemap>();
        lightEmitters = new HashSet<LightEmitter>();
        lightEmittingTiles = new Dictionary<Tile, Coord>();

        map = new Color[tilemap.MapWidth, tilemap.MapHeight];
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

    public Color GetLight(Coord position)
    {
        Vector3 lightAtPoint = AmbientLight.ToVector3();
        
        // TODO: Use quadtrees to take into account only light emitters in range?

        foreach (LightEmitter emitter in lightEmitters)
        {
            LightSource lightSource = emitter.Light;
            if (lightSource is null) continue;
            if (!lightSource.IsInRange(emitter.OwnerEntity.Position, position)) continue;
            
            Vector3 light = lightSource.GetLight(emitter.OwnerEntity.Position, position).ToVector3();
            lightAtPoint += light;
        }
        
        foreach (var pair in lightEmittingTiles)
        {
            LightSource lightSource = pair.Key.LightSource;
            if (lightSource is null) continue;
            if (!lightSource.IsInRange(pair.Value, position)) continue;
            
            Vector3 light = lightSource.GetLight(pair.Value, position).ToVector3();
            lightAtPoint += light;
        }

        float max = Math.Max(lightAtPoint.X, Math.Max(lightAtPoint.Y, lightAtPoint.Z));
        if (max >= 1)
            lightAtPoint /= max;

        return new Color(lightAtPoint);
    }

    public void RenderLightMap(Bounds cullingBounds)
    {
        // TODO: Parallelization eats 100% of CPU, offload this to the GPU (compute shaders?)
        // Parallel.ForEach(tilemap.Coords, coord => map[coord.X, coord.Y] = GetLight(coord));
                
        // TODO: Prepare data and send it to a compute shader to calculate light instead of the code below
        // Tilemap -> array of light attenuation values
        // Light sources -> position and needed data
        
        // TODO: This will only compute lighting in the culling bounds, what if it changes somewhere not visible on the screen and it affects some mechanics?
        for (float y = cullingBounds.TopLeft.Y; y < cullingBounds.BottomRight.Y + 1; y++)
        {
            for (float x = cullingBounds.TopLeft.X; x < cullingBounds.BottomRight.X + 1; x++)
            {
                Coord coord = (Coord)new Vector2(x, y);
                if (!tilemap.IsInBounds(coord)) continue;
                map[coord.X, coord.Y] = GetLight(coord);
            }
        }
    }

    public Color GetRenderedLight(Coord position) => map[position.X, position.Y];
}