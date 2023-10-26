using System;
using System.Collections.Generic;
using MariEngine.Components;
using MariEngine.Exceptions;
using MariEngine.Light;
using MariEngine.Services;
using Microsoft.Xna.Framework;

namespace MariEngine.Tiles;

public class Tilemap : Component
{
    private TileBuffer map;
    
    public HashSet<TileEntity> TileEntities { get; }
    
    private HashSet<TileBehavior> BehaviorsToUpdate { get; }
    
    public int MapWidth { get; private set; }
    public int MapHeight { get; private set; }

    public CoordBounds Bounds => new(Coord.Zero, new Coord(MapWidth, MapHeight));

    public Tilemap(int width, int height)
    {
        map = new TileBuffer(width, height);
        MapHeight = height;
        MapWidth = width;

        TileEntities = new HashSet<TileEntity>();
        BehaviorsToUpdate = new HashSet<TileBehavior>();
    }

    protected override void OnAttach()
    {
        Fill(ServiceRegistry.Get<TileLoader>().Get("Nothing"));
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

    public void Resize(Coord newSize)
    {
        (MapWidth, MapHeight) = newSize;
        map = new TileBuffer(MapWidth, MapHeight);
        GetComponent<LightMap>()?.Resize(newSize);
    }

    public void Place(Tile tile, Coord coord)
    {
        // TODO: Update light map for all tile entity light emitters that affect this tile when tilemap is changed
        if (this[coord] is not null)
        {
            foreach (var behavior in this[coord].Behaviors) BehaviorsToUpdate.Remove(behavior);
            if (this[coord].LightSource is not null)
                GetComponent<LightMap>()?.RemoveEmittingTile(this[coord]);
        }

        Tile newTile = new Tile(tile);
        this[coord] = newTile;
        newTile.OwnerTilemap = this;
        newTile.OnPlaced();
        
        foreach (var behavior in this[coord].Behaviors) BehaviorsToUpdate.Add(behavior);

        if (newTile.LightSource is not null)
        {
            newTile.LightSource.AttachTilemap(this);
            GetComponent<LightMap>()?.AddEmittingTile(newTile, coord);
        }
    }
    
    public void Mine(Coord tileCoord)
    {
        // TODO: Update light map for all tile entity light emitters that affect this tile when tilemap is changed
        this[tileCoord].OnMined();
        
        Place(ServiceRegistry.Get<TileLoader>().Get("Nothing"), tileCoord);
    }

    public void StepOn(TileEntity steppingEntity, Coord tileCoord)
    {
        this[tileCoord].OnSteppedOn(steppingEntity);
    }

    public void Fill(Tile tile)
    {
        for (int y = 0; y < MapHeight; y++)
        {
            for (int x = 0; x < MapWidth; x++)
            {
                Place(tile, new Coord(x, y));
            }
        }
    }

    public void PasteAt(TileBuffer buffer, Coord position)
    {
        foreach (Coord coord in buffer.Coords)
        {
            Coord actualCoord = coord + position;
            if (!IsInBounds(actualCoord)) continue;
            Place(buffer[coord], actualCoord);
        }
    }

    public Tile this[Coord coord]
    {
        get => map[coord];
        private set => map[coord] = value;
    }

    public bool IsInBounds(Coord coord) => map.IsInBounds(coord);

    public Tile this[int x, int y]
    {
        get => this[new Coord(x, y)];
        set => this[new Coord(x, y)] = value;
    }

    public Tile Get(int x, int y)
    {
        return Get(new Coord(x, y));
    }

    public Tile Get(Coord coord)
    {
        if (!IsInBounds(coord))
            throw new OutOfBoundsException(coord);
        return this[coord];
    }

    public void AddTileEntity(TileEntity tileEntity)
    {
        TileEntities.Add(tileEntity);
        tileEntity.AttachToTilemap(this);
    }

    public IEnumerable<Coord> Coords => map.Coords;

    protected override void OnDestroy()
    {
        foreach (TileEntity tileEntity in TileEntities)
            tileEntity.Destroy();
    }
}