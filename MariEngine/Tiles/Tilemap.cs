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
    
    public int MapWidth { get; }
    public int MapHeight { get; }

    public CoordBounds Bounds => new(Coord.Zero, new Coord(MapWidth, MapHeight));

    public Tilemap(int width, int height)
    {
        map = new TileBuffer(width, height);
        MapHeight = height;
        MapWidth = width;

        TileEntities = new HashSet<TileEntity>();
    }

    public override void OnAttach()
    {
        Fill(ServiceRegistry.Get<TileLoader>().GetTile("Nothing"));
    }

    public override void Update(GameTime gameTime)
    {
        // TODO: Optimize this to build a set of behaviors to update
        foreach (Tile tile in map)
        {
            if (tile is null) continue;
            foreach (var behavior in tile.Behaviors)
            {
                behavior.Update(gameTime);
            }
        }

        foreach (TileEntity tileEntity in TileEntities)
        {
            tileEntity.Update(gameTime);
        }
    }

    public void Place(Tile tile, Coord coord)
    {
        if (this[coord] is not null && this[coord].LightSource is not null)
            GetComponent<LightMap>()?.RemoveEmittingTile(this[coord]);

        Tile newTile = new Tile(tile);
        this[coord] = newTile;
        newTile.OwnerTilemap = this;
        newTile.OnPlaced();

        if (newTile.LightSource is not null)
        {
            newTile.LightSource.AttachTilemap(this);
            GetComponent<LightMap>()?.AddEmittingTile(newTile, coord);
        }
    }
    
    public void Mine(Coord tileCoord)
    {
        this[tileCoord].OnMined();
        
        Place(ServiceRegistry.Get<TileLoader>().GetTile("Nothing"), tileCoord);
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

    public void CopyFrom(TileBuffer tiles)
    {
        map.CopyFrom(tiles);
    }

    public void CopyTo(out TileBuffer tiles)
    {
        map.CopyTo(out tiles);
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