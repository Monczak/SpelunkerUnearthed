﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using SpelunkerUnearthed.Engine.Components;
using SpelunkerUnearthed.Engine.Tiles;

namespace SpelunkerUnearthed.Engine.Light;

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

    public void RenderLightMap()
    {
        // TODO: Parallelization eats 100% of CPU, offload this to the GPU (compute shaders?)
        // Parallel.ForEach(tilemap.Coords, coord => map[coord.X, coord.Y] = GetLight(coord));
        foreach (Coord coord in tilemap.Coords)
        {
            map[coord.X, coord.Y] = GetLight(coord);
        }
    }

    public Color GetRenderedLight(Coord position) => map[position.X, position.Y];
}