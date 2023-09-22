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
    private Tile[,] map;
    
    public HashSet<TileEntity> TileEntities { get; }
    
    public int MapWidth { get; }
    public int MapHeight { get; }

    public Bounds Bounds => new(Vector2.Zero, new Vector2(MapWidth, MapHeight));

    public Tilemap(int width, int height)
    {
        map = new Tile[height, width];
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

    public void CopyFrom(Tile[,] tiles)
    {
        if (tiles.GetLength(0) != map.GetLength(0) || tiles.GetLength(1) != map.GetLength(1))
            throw new ArgumentException($"Tilemaps must be same size");
        
        for (int y = 0; y < MapHeight; y++)
        {
            for (int x = 0; x < MapWidth; x++)
            {
                Place(new Tile(tiles[x, y]), new Coord(x, y));
            }
        }
    }

    public void CopyTo(out Tile[,] tiles)
    {
        tiles = new Tile[MapWidth, MapHeight];
        for (int y = 0; y < MapHeight; y++)
        {
            for (int x = 0; x < MapWidth; x++)
            {
                tiles[x, y] = map[x, y];
            }
        }
    }

    public Tile this[Coord coord]
    {
        get
        {
            if (!IsInBounds(coord))
                throw new OutOfBoundsException(coord);
            return map[coord.Y, coord.X];
        }
        set
        {
            if (!IsInBounds(coord))
                throw new OutOfBoundsException(coord);
            map[coord.Y, coord.X] = value;
        }
    }

    public bool IsInBounds(Coord coord)
    {
        return coord.X >= 0 && coord.X < MapWidth && coord.Y >= 0 && coord.Y < MapHeight;
    }

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

    public IEnumerable<Coord> Coords
    {
        get
        {
            for (int y = 0; y < MapHeight; y++)
            {
                for (int x = 0; x < MapWidth; x++)
                {
                    yield return new Coord(x, y);
                }
            }
        }
    }

    protected override void OnDestroy()
    {
        foreach (TileEntity tileEntity in TileEntities)
            tileEntity.Destroy();
    }
}