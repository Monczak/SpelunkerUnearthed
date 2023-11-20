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

    private HashSet<LightSourceData> dirtyLightSources = new();
    private HashSet<LightSourceData> toRemove = new();

    private Vector3[,] map;

    public int RenderThreads { get; set; } = 4;

    private Action<LightSource> dirtyLightAction;

    private class LightSourceData
    {
        public LightSource LightSource { get; }
        public Deferred<Coord> Position { get; }
        
        public bool New { get; set; }
        public bool Old { get; set; }
        
        public Vector3[,] RenderedLight;

        public LightSourceData(LightSource lightSource, Coord position)
        {
            LightSource = lightSource;
            Position = new Deferred<Coord>(position);
            New = true;
            Old = false;

            var bounds = lightSource.GetBounds(Coord.Zero);
            if (bounds is not null)
                RenderedLight = new Vector3[bounds.Value.Size.X, bounds.Value.Size.Y];
            else
                RenderedLight = new Vector3[1, 1]; // TODO: What to do with unbounded lights?
        }
    }

    protected override void OnAttach()
    {
        tilemap = GetComponent<Tilemap>();
        lightSources = new Dictionary<LightSource, LightSourceData>();
        toRemove = new HashSet<LightSourceData>();
        dirtyLightSources = new HashSet<LightSourceData>();

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
        foreach (LightSourceData data in toRemove)
        {
            data.LightSource.OnDirty -= dirtyLightAction;
            lightSources.Remove(data.LightSource);
        }
        toRemove.Clear();
    }

    private void UpdateDirtyLights()
    {
        foreach (LightSourceData data in dirtyLightSources)
        {
            if (!data.New)
            {
                RenderLight(data, derender: true);
            }
            else
            {
                toRemove.Remove(data);
            }

            data.Position.Update();
            data.LightSource.UpdateAllProperties();
                
            if (!data.Old)
            {
                RenderLight(data);
            }
            else
            {
                toRemove.Add(data);
            }

            data.New = false;
            data.LightSource.Dirty = false;
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
        AddLightSource(new LightSourceData(tile.LightSource, position));
    }

    public void RemoveEmittingTile(Tile tile)
    {
        RemoveLightSource(tile.LightSource);
    }

    public void AddEmitter(LightEmitter emitter)
    {
        AddLightSource(new LightSourceData(emitter.LightSource, emitter.OwnerEntity.Position));
    }

    public void RemoveEmitter(LightEmitter emitter)
    {
        RemoveLightSource(emitter.LightSource);
    }

    private void AddLightSource(LightSourceData data)
    {
        lightSources.Add(data.LightSource, data);
        data.LightSource.OnDirty += dirtyLightAction;
        
        data.LightSource.Dirty = true;
    }

    private void RemoveLightSource(LightSource lightSource)
    {
        if (lightSources.TryGetValue(lightSource, out var sourceData))
        {
            sourceData.Old = true;
            lightSource.Dirty = true;
        }
    }

    private void OnLightSourceDirty(LightSource source) => dirtyLightSources.Add(lightSources[source]);

    private void RenderLight(LightSourceData data, bool derender = false)
    {
        CoordBounds? lightBounds = data.LightSource.GetBounds(data.Position.Get());
        if (lightBounds is null) return;    // TODO: Support unbounded (global) lights
        
        for (int x = lightBounds.Value.TopLeft.X; x <= lightBounds.Value.BottomRight.X; x++)
        {
            for (int y = lightBounds.Value.TopLeft.Y; y <= lightBounds.Value.BottomRight.Y; y++)
            {
                Coord coord = new(x, y);
                if (!tilemap.IsInBounds(coord)) continue;

                int cx = x - lightBounds.Value.TopLeft.X;
                int cy = y - lightBounds.Value.TopLeft.Y;

                Vector3 light;
                if (derender)
                    light = data.RenderedLight[cx, cy];
                else
                {
                    light = data.LightSource.GetLight(data.Position.Get(), coord).ToVector3();
                    data.RenderedLight[cx, cy] = light;
                }
                
                map[x, y] += light * (derender ? -1 : 1);
                map[x, y].X = MathF.Max(0, map[x, y].X);
                map[x, y].Y = MathF.Max(0, map[x, y].Y);
                map[x, y].Z = MathF.Max(0, map[x, y].Z);
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