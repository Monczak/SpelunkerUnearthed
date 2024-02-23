using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using MariEngine.Exceptions;
using MariEngine.Persistence;
using MariEngine.Services;
using MariEngine.Utils;
using YamlDotNet.Serialization;

namespace MariEngine.Tiles;

public class TileBuffer(int width, int height) : IEnumerable<Tile>, ISaveable<TileBuffer>
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

    public TileBufferFragment GetFragment(CoordBounds bounds) => new(this, bounds);
    
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
    
    public void PasteAt(TileBuffer buffer, Coord position, bool maskNulls = true)
    {
        foreach (Coord coord in buffer.Coords)
        {
            Coord actualCoord = coord + position;
            if (!IsInBounds(actualCoord)) continue;
            if (maskNulls && buffer[coord] is null) continue;

            this[actualCoord] = buffer[coord];
        }
    }

    private struct TileBufferInfo
    {
        public required int Width { get; init; }
        public required int Height { get; init; }
        public required Dictionary<string, string> Keys { get; init; }
        public required string Map { get; init; }
    }

    // TODO: Improve serialization/deserialization (for now it's text-based, switch to an adaptable binary format)
    public void Serialize(Stream stream)
    {
        var writer = new StreamWriter(stream);
        var keys = ShortKeyGen.GetKeys(map.Select(tile => tile.Id));
        var serializedData = new SerializerBuilder()
            .Build()
            .Serialize(new TileBufferInfo
            {
                Width = width,
                Height = height,
                Map = string.Join("|", map.Select(tile => keys[tile.Id])),
                Keys = keys.ToDictionary(pair => pair.Value, pair => pair.Key)
            });
        writer.Write(serializedData);
        writer.Flush();
    }

    public static TileBuffer Deserialize(Stream stream)
    {
        var reader = new StreamReader(stream);
        var data = new DeserializerBuilder()
            .Build()
            .Deserialize<TileBufferInfo>(reader.ReadToEnd());

        var buffer = new TileBuffer(data.Width, data.Height);
        var tileIds = data.Map.Split("|").Select(id => data.Keys[id]).ToList();

        if (tileIds.Count != buffer.Width * buffer.Height)
            throw new InvalidDataException($"Serialized map data length does not match buffer size (expected {buffer.Width * buffer.Height}, got {tileIds.Count}).");
        
        int i = 0;
        foreach (Coord coord in buffer.Coords)
            buffer[coord] = ServiceRegistry.Get<TileLoader>().Get(tileIds[i++]);

        return buffer;
    }
}