using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public Color AmbientLight { get; set; }

    // This WILL BREAK LIGHTING if not set, unless we come up with a neater method to calculate/store light rendered before world updates
    // (the memory savings, tho... D:)
    private readonly bool cacheRenderedLight;
    
    private Dictionary<LightSource, LightSourceData> lightSources;
    private Dictionary<Coord, LightSourceData> staticLightSources;

    private HashSet<LightSourceData> dirtyLightSources = [];
    private HashSet<LightSourceData> toRemove = [];

    // private SpatialPartition<LightSource> spatialPartition;
    private Coord? spatialPartitionCellSize;
    
    private Vector3[,] map;

    public int RenderThreads { get; set; } = 8;

    private readonly Action<LightSource> dirtyLightHandler;

    private object lockObj = new();

    private class LightSourceData
    {
        public LightSource LightSource { get; }
        public Deferred<Coord> Position { get; }
        
        public bool New { get; set; }
        public bool Old { get; set; }

        public Vector3[,] RenderedLight;

        public LightSourceData(LightSource lightSource, Coord position, bool useCache)
        {
            LightSource = lightSource;
            Position = new Deferred<Coord>(position);
            New = true;
            Old = false;

            var bounds = lightSource.GetBounds(Coord.Zero);
            if (useCache)
            {
                if (bounds is not null)
                    RenderedLight = new Vector3[bounds.Value.Size.X, bounds.Value.Size.Y];
                else
                    RenderedLight = new Vector3[1, 1]; // TODO: What to do with unbounded lights?
            }
        }
    }

    public LightMap(Coord? spatialPartitionCellSize = null, bool cacheRenderedLight = true)
    {
        dirtyLightHandler = OnLightSourceDirty;
        this.spatialPartitionCellSize = spatialPartitionCellSize;
        this.cacheRenderedLight = cacheRenderedLight;
    }

    protected internal override void Initialize()
    {
        tilemap = GetComponent<Tilemap>();
        lightSources = new Dictionary<LightSource, LightSourceData>();
        staticLightSources = new Dictionary<Coord, LightSourceData>();

        toRemove = [];
        dirtyLightSources = [];

        map = new Vector3[tilemap.Width, tilemap.Height];
    }

    protected override void OnDestroy()
    {
        foreach (var (lightSource, lightSourceData) in lightSources)
        {
            lightSource.OnDirty -= dirtyLightHandler;
            lightSourceData.RenderedLight = null;
        }

        map = null;
        base.OnDestroy();
    }

    public void Resize(Coord newSize)
    {
        map = new Vector3[newSize.X, newSize.Y];
    }

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        
        UpdateDirtyLights();
        RemoveDirtyLights();
    }

    private void RemoveDirtyLights()
    {
        foreach (LightSourceData data in toRemove)
        {
            data.LightSource.OnDirty -= dirtyLightHandler;
            lightSources.Remove(data.LightSource);
        }
        toRemove.Clear();
    }

    private void UpdateDirtyLights()
    {
        object lockObj2 = new();
        Parallel.ForEach(dirtyLightSources, new ParallelOptions { MaxDegreeOfParallelism = RenderThreads },data =>
        {
            if (!data.New)
            {
                RenderLight(data, derender: true);
            }
            else
            {
                lock (lockObj2)
                {
                    toRemove.Remove(data);
                }
            }

            data.Position.Update();
            data.LightSource.UpdateAllProperties();

            if (!data.Old)
            {
                RenderLight(data);
            }
            else
            {
                lock (lockObj2)
                {
                    toRemove.Add(data);
                }
            }

            data.New = false;
            data.LightSource.Dirty = false;
        });
        dirtyLightSources.Clear();
    }

    public void ForceUpdate()
    {
        UpdateDirtyLights();
    }

    public void UpdatePosition(LightSource source, Coord position)
    {
        lightSources[source].Position.Set(position);
        source.Dirty = true;
    }

    public void AddEmittingTile(Tile tile, Coord position)
    {
        AddLightSource(new LightSourceData(tile.LightSource, position, cacheRenderedLight), true);
    }

    public void RemoveEmittingTile(Coord position)
    {
        RemoveStaticLightSource(position);
    }

    public void AddEmitter(LightEmitter emitter)
    {
        AddLightSource(new LightSourceData(emitter.LightSource, emitter.OwnerEntity.Position, cacheRenderedLight));
    }

    public void RemoveEmitter(LightEmitter emitter)
    {
        RemoveLightSource(emitter.LightSource);
    }

    private void AddLightSource(LightSourceData data, bool staticSource = false)
    {
        lock (lockObj)
        {
            if (staticSource) staticLightSources.Add(data.Position.Get(), data);
            else lightSources.Add(data.LightSource, data);
            data.LightSource.OnDirty += dirtyLightHandler;
        
            if (staticSource)
                MakeStaticLightSourceDirty(data.Position.Get());
            else 
                data.LightSource.Dirty = true;
        }
    }

    private void RemoveLightSource(LightSource lightSource)
    {
        if (lightSources.TryGetValue(lightSource, out var sourceData))
        {
            sourceData.Old = true;
            lightSource.Dirty = true;
        }
    }

    private void RemoveStaticLightSource(Coord position)
    {
        if (staticLightSources.TryGetValue(position, out var sourceData))
        {
            sourceData.Old = true;
            MakeStaticLightSourceDirty(position);
        }
    }

    private void OnLightSourceDirty(LightSource source) => dirtyLightSources.Add(lightSources[source]);

    private void MakeStaticLightSourceDirty(Coord position)
    {
        dirtyLightSources.Add(staticLightSources[position]);
    }

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
                if (derender && cacheRenderedLight)
                    light = data.RenderedLight[cx, cy];
                else
                {
                    light = data.LightSource.GetLight(tilemap, data.Position.Get(), coord).ToVector3();
                    if (cacheRenderedLight) data.RenderedLight[cx, cy] = light;
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
        if (!tilemap.IsInBounds(position)) return Color.Black;
        
        Vector3 color = map[position.X, position.Y] + AmbientLight.ToVector3();
        float max = MathF.Max(color.X, MathF.Max(color.Y, color.Z));
        if (max > 1)
            color /= max;
        return new Color(color);
    }
}