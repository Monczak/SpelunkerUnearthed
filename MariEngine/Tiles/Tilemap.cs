﻿using System;
using System.Collections.Generic;
using MariEngine.Components;
using MariEngine.Exceptions;
using MariEngine.Light;
using MariEngine.Services;
using Microsoft.Xna.Framework;

namespace MariEngine.Tiles;

public class Tilemap : Component
{
    private SortedDictionary<int, TileBuffer> layers;

    public const int BaseLayer = 0;
    public const int GroundLayer = -1;

    private List<int> layerIds = new();
    
    public HashSet<TileEntity> TileEntities { get; }
    
    private HashSet<TileBehavior> BehaviorsToUpdate { get; }
    
    public int Width { get; private set; }
    public int Height { get; private set; }

    public CoordBounds Bounds => new(Coord.Zero, new Coord(Width, Height));

    private class DescendingComparer<T> : IComparer<T> where T : IComparable<T>
    {
        public int Compare(T x, T y)
        {
            return y.CompareTo(x);
        }
    }
    
    public Tilemap(int width, int height)
    {
        Width = width;
        Height = height;

        layers = new SortedDictionary<int, TileBuffer>(new DescendingComparer<int>());
        AddLayer(BaseLayer);

        TileEntities = new HashSet<TileEntity>();
        BehaviorsToUpdate = new HashSet<TileBehavior>();
    }

    protected override void OnAttach()
    {
        foreach (var pair in layers)
        {
            Fill(ServiceRegistry.Get<TileLoader>().Get("Nothing"), pair.Key);
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

    public void AddLayer(int layerId)
    {
        layers.Add(layerId, new TileBuffer(Width, Height));
        layerIds = new List<int>(layers.Keys);
    }

    public void Resize(Coord newSize)
    {
        (Width, Height) = newSize;
        foreach (int layerId in layerIds) layers[layerId] = new TileBuffer(Width, Height);

        GetComponent<LightMap>()?.Resize(newSize);
    }

    public void Place(Tile tile, Coord coord, int layerId)
    {
        Tile theTile = Get(coord, layerId);
        // TODO: Update light map for all tile entity light emitters that affect this tile when tilemap is changed
        if (theTile is not null)
        {
            foreach (var behavior in theTile.Behaviors) BehaviorsToUpdate.Remove(behavior);
            if (theTile.LightSource is not null)
                GetComponent<LightMap>()?.RemoveEmittingTile(theTile);
        }

        Tile newTile = new Tile(tile);
        Set(newTile, coord, layerId);
        newTile.OwnerTilemap = this;
        newTile.OnPlaced();
        
        foreach (var behavior in Get(coord, layerId).Behaviors) BehaviorsToUpdate.Add(behavior);

        if (newTile.LightSource is not null)
        {
            newTile.LightSource.AttachTilemap(this);
            GetComponent<LightMap>()?.AddEmittingTile(newTile, coord);
        }
    }
    
    public void Mine(Coord tileCoord, int layerId)
    {
        // TODO: Update light map for all tile entity light emitters that affect this tile when tilemap is changed
        Get(tileCoord, layerId).OnMined();
        
        Place(ServiceRegistry.Get<TileLoader>().Get("Nothing"), tileCoord, layerId);
    }

    public void StepOn(TileEntity steppingEntity, Coord tileCoord)
    {
        foreach (int layerId in layers.Keys)
        {
            Get(tileCoord, layerId)?.OnSteppedOn(steppingEntity);    // TODO: Step only on tiles below the base layer?
        }
    }

    public void Fill(Tile tile, int layerId)
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                Place(tile, new Coord(x, y), layerId);
            }
        }
    }

    public void PasteAt(TileBuffer buffer, Coord position, int layerId)
    {
        foreach (Coord coord in buffer.Coords)
        {
            Coord actualCoord = coord + position;
            if (!IsInBounds(actualCoord)) continue;
            Place(buffer[coord], actualCoord, layerId);
        }
    }

    public Tile Get(Coord coord, int layerId)
    {
        if (!layers.TryGetValue(layerId, out var layer))
            throw new ArgumentException($"Layer {layerId} does not exist.");

        if (!IsInBounds(coord))
            throw new OutOfBoundsException(coord);
        
        return layer[coord];
    }

    public Tile GetTop(Coord coord)
    {
        foreach (int layerId in layerIds)
        {
            Tile tile = layers[layerId][coord];
            if (tile is null || tile.Id != "Nothing")
                return Get(coord, layerId);
        }
        return Get(coord, BaseLayer);
    }

    public void Set(Tile tile, Coord coord, int layerId)
    {
        if (!layers.TryGetValue(layerId, out var layer))
            throw new ArgumentException($"Layer {layerId} does not exist.");

        if (!IsInBounds(coord))
            throw new OutOfBoundsException(coord);

        layer[coord] = tile;
    }

    public bool IsInBounds(Coord coord) => Bounds.PointInside(coord);

    public void AddTileEntity(TileEntity tileEntity)
    {
        TileEntities.Add(tileEntity);
        tileEntity.AttachToTilemap(this);
    }

    public IEnumerable<Coord> Coords => Bounds.Coords;

    protected override void OnDestroy()
    {
        foreach (TileEntity tileEntity in TileEntities)
            tileEntity.Destroy();
    }
}