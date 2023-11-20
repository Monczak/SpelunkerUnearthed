using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using MariEngine.Components;
using MariEngine.Logging;
using MariEngine.Rendering;
using MariEngine.Services;
using MariEngine.Tiles;
using MariEngine.Utils;
using Microsoft.Xna.Framework;

namespace MariEngine.Light;

public class LightMap : Component
{
    private Tilemap tilemap;
    private TilemapRenderer tilemapRenderer;
    public Color AmbientLight { get; set; }
    
    private Dictionary<LightSource, LightSourceData> lightSources;

    private HashSet<LightSource> dirtyLightSources = new();
    private HashSet<LightSource> toRemove = new();

    private Vector3[,] map;

    public int RenderThreads { get; set; } = 4;

    private Action<LightSource> dirtyLightAction;

    private class LightSourceData
    {
        public Deferred<Coord> Position { get; }
        
        public bool New { get; set; }
        public bool Old { get; set; }
        
        // TODO: Maybe there's a better way to store rendered light?
        public Dictionary<(int x, int y), Vector3> RenderedLight { get; }

        public LightSourceData(Coord position)
        {
            Position = new Deferred<Coord>(position);
            New = true;
            Old = false;

            RenderedLight = new Dictionary<(int x, int y), Vector3>();
        }
    }

    protected override void OnAttach()
    {
        tilemap = GetComponent<Tilemap>();
        lightSources = new Dictionary<LightSource, LightSourceData>();
        toRemove = new HashSet<LightSource>();
        dirtyLightSources = new HashSet<LightSource>();

        map = new Vector3[tilemap.MapWidth, tilemap.MapHeight];

        dirtyLightAction = OnLightSourceDirty;
    }

    public void Resize(Coord newSize)
    {
        map = new Vector3[newSize.X, newSize.Y];
    }

    public void AttachTilemapRenderer(TilemapRenderer renderer)
    {
        tilemapRenderer = renderer;
    }

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        
        UpdateDirtyLights();
        RemoveLights();
    }

    private void RemoveLights()
    {
        foreach (LightSource source in toRemove)
        {
            source.OnDirty -= dirtyLightAction;
            lightSources.Remove(source);
        }
    }

    private void UpdateDirtyLights()
    {
        foreach (LightSource source in dirtyLightSources)
        {
            if (!lightSources[source].New)
            {
                RenderLight(source, lightSources[source].Position.Get(), derender: true);
            }
            else
            {
                toRemove.Remove(source);
            }

            lightSources[source].Position.Update();
            source.UpdateAllProperties();
                
            if (!lightSources[source].Old)
            {
                RenderLight(source, lightSources[source].Position.Get());
            }
            else
            {
                toRemove.Add(source);
            }

            lightSources[source].New = false;
            source.Dirty = false;
        }
        dirtyLightSources.Clear();
    }

    public void UpdatePosition(LightSource source, Coord position)
    {
        lightSources[source].Position.Set(position);
        source.Dirty = true;
    }

    public void AddEmittingTile(Tile tile, Coord position)
    {
        AddLightSource(tile.LightSource, new LightSourceData(position));
    }

    public void RemoveEmittingTile(Tile tile)
    {
        RemoveLightSource(tile.LightSource);
    }

    public void AddEmitter(LightEmitter emitter)
    {
        AddLightSource(emitter.LightSource, new LightSourceData(emitter.OwnerEntity.Position));
    }

    public void RemoveEmitter(LightEmitter emitter)
    {
        RemoveLightSource(emitter.LightSource);
    }

    private void AddLightSource(LightSource lightSource, LightSourceData data)
    {
        lightSources.Add(lightSource, data);
        lightSource.OnDirty += dirtyLightAction;
        
        lightSource.Dirty = true;
    }

    private void RemoveLightSource(LightSource lightSource)
    {
        if (lightSources.TryGetValue(lightSource, out var sourceData))
        {
            sourceData.Old = true;
            lightSource.Dirty = true;
        }
    }

    private void OnLightSourceDirty(LightSource source) => dirtyLightSources.Add(source);

    private void RenderLight(LightSource source, Coord position, bool derender = false)
    {
        CoordBounds? lightBounds = source.GetBounds(position);
        if (lightBounds is null) return;    // TODO: Support unbounded (global) lights
        
        if (!derender)
            lightSources[source].RenderedLight.Clear();
        
        for (int x = lightBounds.Value.TopLeft.X; x <= lightBounds.Value.BottomRight.X; x++)
        {
            for (int y = lightBounds.Value.TopLeft.Y; y <= lightBounds.Value.BottomRight.Y; y++)
            {
                Coord coord = (Coord)new Vector2(x, y);
                if (!tilemap.IsInBounds(coord)) continue;

                Vector3 light;
                if (derender)
                    light = lightSources[source].RenderedLight[(coord.X, coord.Y)];
                else
                {
                    light = source.GetLight(position, coord).ToVector3();
                    lightSources[source].RenderedLight[(coord.X, coord.Y)] = light;
                }
                
                map[coord.X, coord.Y] += light * (derender ? -1 : 1);
                map[coord.X, coord.Y].X = MathF.Max(0, map[coord.X, coord.Y].X);
                map[coord.X, coord.Y].Y = MathF.Max(0, map[coord.X, coord.Y].Y);
                map[coord.X, coord.Y].Z = MathF.Max(0, map[coord.X, coord.Y].Z);
            }
        }
    }

    public Color GetRenderedLight(Coord position)
    {
        Vector3 color = map[position.X, position.Y] + AmbientLight.ToVector3();
        float max = MathF.Max(color.X, MathF.Max(color.Y, color.Z));
        if (max > 1)
            color /= max;
        return new Color(color);
    }

    public Vector3 GetLight(Coord position)
    {
        return map[position.X, position.Y];
    }
}