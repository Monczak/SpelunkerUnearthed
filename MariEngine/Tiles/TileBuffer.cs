using System;
using System.Collections;
using System.Collections.Generic;
using MariEngine.Exceptions;

namespace MariEngine.Tiles;

public class TileBuffer(int width, int height) : IEnumerable<Tile>
{
    private readonly Tile[] map = new Tile[width * height];
    
    public int Width { get; private set; } = width;
    public int Height { get; private set; } = height;

    public TileBuffer(Coord size) : this(size.X, size.Y)
    {
        
    }

    public Tile this[int x, int y]
    {
        get
        {
            if (!IsInBounds(x, y)) throw new OutOfBoundsException(x, y);
            return map[y * Width + x];
        }
        set
        {
            if (!IsInBounds(x, y)) throw new OutOfBoundsException(x, y);
            map[y * Width + x] = value;
        }
    }

    public Tile this[Coord coord]
    {
        get => this[coord.X, coord.Y];
        set => this[coord.X, coord.Y] = value;
    }

    public bool IsInBounds(Coord coord) => IsInBounds(coord.X, coord.Y);
    
    public bool IsInBounds(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }

    public IEnumerator<Tile> GetEnumerator() => ((IEnumerable<Tile>)map).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
    public IEnumerable<Coord> Coords
    {
        get
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    yield return new Coord(x, y);
                }
            }
        }
    }

    public void CopyTo(out TileBuffer buffer)
    {
        buffer = new TileBuffer(Width, Height);
        foreach (Coord coord in Coords)
            buffer[coord] = this[coord];
    }
    
    public void CopyFrom(TileBuffer buffer)
    {
        if (buffer.Width != Width || buffer.Height != Height)
            throw new ArgumentException($"Tilemaps must be same size");
        
        foreach (Coord coord in Coords)
            this[coord] = buffer[coord];
    }
}