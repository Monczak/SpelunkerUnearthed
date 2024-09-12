using System;
using System.Collections;
using System.Collections.Generic;
using MariEngine.Components;
using MariEngine.Exceptions;
using MariEngine.Light;
using MariEngine.Services;
using Microsoft.Xna.Framework;

namespace MariEngine.Tiles;

public class Tilemap : Component
{
    private Transform transform;
    private SortedList<TilemapLayer, TileBuffer> layers;

    private List<TilemapLayer> layerIds = [];
    
    public SortedSet<TileEntity> TileEntities { get; }
    
    private HashSet<TileBehavior> BehaviorsToUpdate { get; }
    
    public int Width { get; private set; }
    public int Height { get; private set; }

    public CoordBounds Bounds => new(Coord.Zero, new Coord(Width, Height));

    public event Action<TileEntity> TileEntityAdded;
    public event Action<TileEntity> TileEntityRemoved;

    public delegate void TileUpdateDelegate(Coord position, Tile tile);

    public event TileUpdateDelegate TileMined;

    private class TilemapLayerDescendingComparer : IComparer<TilemapLayer>
    {
        public int Compare(TilemapLayer x, TilemapLayer y)
        {
            return ((int)y).CompareTo((int)x);
        }
    }
    
    public Tilemap(int width, int height)
    {
        Width = width;
        Height = height;

        layers = new SortedList<TilemapLayer, TileBuffer>(new TilemapLayerDescendingComparer());
        AddLayer(TilemapLayer.Base);

        TileEntities = new SortedSet<TileEntity>(new PriorityComparer<TileEntity>());
        BehaviorsToUpdate = [];
    }

    protected internal override void Initialize()
    {
        base.Initialize();
        
        transform = GetComponent<Transform>();
        
        foreach (var pair in layers)
        {
            Fill(ServiceRegistry.Get<TileLoader>().Get("Nothing"), pair.Key);
        }
        
        foreach (var tileEntity in TileEntities)
        {
            tileEntity.InitializeComponents();
        }
    }
    
    protected override void Update(GameTime gameTime)
    {
        foreach (var behavior in BehaviorsToUpdate)
        {
            // TODO: Check if the behavior is in the update range (decide how it will be implemented)
            behavior.Update(gameTime);
        }

        foreach (var tileEntity in TileEntities)
        {
            tileEntity.Update(gameTime);
        }
    }

    public void AddLayer(TilemapLayer layerId)
    {
        layers.Add(layerId, new TileBuffer(Width, Height));
        layerIds = [..layers.Keys];
    }

    public TileBuffer GetLayer(TilemapLayer layerId) => layers[layerId];

    public void Resize(Coord newSize)
    {
        (Width, Height) = newSize;
        foreach (var layerId in layerIds) layers[layerId] = new TileBuffer(Width, Height);

        var lightMap = GetComponent<LightMap>();
        lightMap?.Resize(newSize);
    }

    public void Place(Tile tile, Coord coord, TilemapLayer layerId)
    {
        Tile theTile = Get(coord, layerId);
        // TODO: Update light map for all tile entity light emitters that affect this tile when tilemap is changed
        if (theTile is not null)
        {
            foreach (var behavior in theTile.Behaviors) BehaviorsToUpdate.Remove(behavior);
            if (theTile.LightSource is not null)
                GetComponent<LightMap>()?.RemoveEmittingTile(coord);
        }
        
        Set(tile, coord, layerId);
        tile.OnPlaced(coord);
        
        foreach (var behavior in Get(coord, layerId).Behaviors) BehaviorsToUpdate.Add(behavior);

        if (tile.LightSource is not null)
        {
            GetComponent<LightMap>()?.AddEmittingTile(tile, coord);
        }
    }
    
    public bool Mine(Coord tileCoord, TilemapLayer layerId)
    {
        // TODO: Update light map for all tile entity light emitters that affect this tile when tilemap is changed
        var tile = Get(tileCoord, layerId);
        tile?.OnMined(tileCoord);
        
        var wasEmpty = tile is null || tile.Id == "Nothing";
        
        Place(ServiceRegistry.Get<TileLoader>().Get("Nothing"), tileCoord, layerId);
        
        if (!wasEmpty)
            TileMined?.Invoke(tileCoord, tile);

        return !wasEmpty;
    }

    public void StepOn(TileEntity steppingEntity, Coord tileCoord)
    {
        foreach (var layerId in layers.Keys)
        {
            Get(tileCoord, layerId)?.OnSteppedOn(tileCoord, steppingEntity);    // TODO: Step only on tiles below the base layer?
        }
    }

    public void Fill(Tile tile, TilemapLayer layerId)
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                Place(tile, new Coord(x, y), layerId);
            }
        }
    }

    public void PasteAt(TileBuffer buffer, Coord position, TilemapLayer layerId, bool maskNulls = true)
    {
        foreach (Coord coord in buffer.Coords)
        {
            Coord actualCoord = coord + position;
            if (!IsInBounds(actualCoord)) continue;
            if (maskNulls && buffer[coord] is null) continue;
            
            Place(buffer[coord], actualCoord, layerId);
        }
    }

    public Tile Get(Coord coord, TilemapLayer layerId)
    {
        if (!layers.TryGetValue(layerId, out var layer))
            throw new ArgumentException($"Layer {layerId} does not exist.");

        if (!IsInBounds(coord))
            throw new OutOfBoundsException(coord);
        
        return layer[coord];
    }

    public Tile GetTop(Coord coord, out TilemapLayer layer)
    {
        foreach (var layerId in layerIds)
        {
            layer = layerId;
            var tile = layers[layerId][coord];
            if (tile is null || tile.Id != "Nothing")
                return Get(coord, layerId);
        }
        layer = TilemapLayer.Base;
        return Get(coord, TilemapLayer.Base);
    }

    public void Set(Tile tile, Coord coord, TilemapLayer layerId)
    {
        if (!layers.TryGetValue(layerId, out var layer))
            throw new ArgumentException($"Layer {layerId} does not exist.");

        if (!IsInBounds(coord))
            throw new OutOfBoundsException(coord);

        layer[coord] = tile;
    }

    public bool IsInBounds(Coord coord) => Bounds.PointInside(coord);

    public TileBufferFragment GetFragment(CoordBounds bounds, TilemapLayer layerId) => layers[layerId].GetFragment(bounds);

    public void AddTileEntity(TileEntity tileEntity)
    {
        TileEntities.Add(tileEntity);
        tileEntity.AttachToTilemap(this);
        TileEntityAdded?.Invoke(tileEntity);
    }

    public void RemoveTileEntity(TileEntity tileEntity)
    {
        TileEntities.Remove(tileEntity);
        TileEntityRemoved?.Invoke(tileEntity);
    }

    public IEnumerable<Coord> Coords => Bounds.Coords;
    
    
    public Vector2 CoordToWorldPoint(Coord coord)
    {
        return Vector2ToWorldPoint((Vector2)coord);
    }
    
    public Vector2 Vector2ToWorldPoint(Vector2 v)
    {
        return v + transform.Position + CalculateCenterOffset();
    }

    public Coord WorldPointToCoord(Vector2 point)
    {
        return (Coord)(point - CalculateCenterOffset() - transform.Position);
    }
    
    internal Vector2 CalculateCenterOffset() => -new Vector2(Width / 2f, Height / 2f);

    protected override void OnDestroy()
    {
        foreach (var tileEntity in TileEntities)
            tileEntity.DestroyWithoutRemove();
        
        TileEntities.Clear();
        layers.Clear();
    }
}