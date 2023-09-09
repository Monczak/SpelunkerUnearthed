using Microsoft.Xna.Framework;
using SpelunkerUnearthed.Engine.Components;
using SpelunkerUnearthed.Engine.Exceptions;
using SpelunkerUnearthed.Engine.Services;

namespace SpelunkerUnearthed.Engine.Tiles;

public class Tilemap : Component
{
    private Tile[,] map;
    
    public int MapWidth { get; }
    public int MapHeight { get; }

    public Tilemap(int width, int height)
    {
        map = new Tile[height, width];
        MapHeight = height;
        MapWidth = width;
    }

    public override void Update(GameTime gameTime)
    {
        Fill(ServiceRegistry.Get<TileLoader>().GetTile("Nothing"));
        this[0, 0] = ServiceRegistry.Get<TileLoader>().GetTile("Stone");
    }

    public void Fill(Tile tile)
    {
        for (int y = 0; y < MapHeight; y++)
        {
            for (int x = 0; x < MapWidth; x++)
            {
                this[x, y] = tile;
            }
        }
    }

    public Tile this[Coord coord]
    {
        get
        {
            if (!IsInBounds(coord))
                return null;
            return map[coord.Y, coord.X];
        }
        set
        {
            if (!IsInBounds(coord))
                throw new OutOfBoundsException(coord);
            map[coord.Y, coord.X] = value;
        }
    }

    private bool IsInBounds(Coord coord)
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
            return null;
        return this[coord];
    }
}