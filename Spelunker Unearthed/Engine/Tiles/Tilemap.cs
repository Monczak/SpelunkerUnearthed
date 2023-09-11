using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SpelunkerUnearthed.Engine.Components;
using SpelunkerUnearthed.Engine.Exceptions;
using SpelunkerUnearthed.Engine.Services;

namespace SpelunkerUnearthed.Engine.Tiles;

public class Tilemap : Component
{
    private Tile[,] map;
    
    public HashSet<TileEntity> TileEntities { get; }
    
    public int MapWidth { get; }
    public int MapHeight { get; }

    public Tilemap(int width, int height)
    {
        map = new Tile[height, width];
        MapHeight = height;
        MapWidth = width;

        TileEntities = new HashSet<TileEntity>();
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
        
        // TODO: Remove this, this is for testing only
        Fill(ServiceRegistry.Get<TileLoader>().GetTile("Stone"));
    }

    public void Place(Tile tile, Coord coord)
    {
        this[coord] = tile;
        tile.OwnerTilemap = this;
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
}