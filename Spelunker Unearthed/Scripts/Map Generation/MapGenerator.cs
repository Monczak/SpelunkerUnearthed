using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MariEngine;
using MariEngine.Components;
using MariEngine.Logging;
using MariEngine.Services;
using MariEngine.Tiles;
using MariEngine.Utils;
using SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;
using SpelunkerUnearthed.Scripts.MapGeneration.MapProcessors;
using Random = MariEngine.Utils.Random;

namespace SpelunkerUnearthed.Scripts.MapGeneration;

public class MapGenerator : Component
{
    private Tilemap tilemap;
    private TileBuffer buffer;

    private Random random;

    private List<MapProcessor> processors = new();

    public int BaseTilemapSize { get; private set; } = 16;

    protected override void OnAttach()
    {
        tilemap = GetComponent<Tilemap>();
    }

    public void AddProcessor<T>(T processor) where T : MapProcessor
    {
        processors.Add(processor);
    }   
    
    public void GenerateMap(Room room, MapGenerationParameters parameters)
    {
        random = ServiceRegistry.Get<RandomProvider>().Request(Constants.MapGen).Seed(parameters.Seed);

        BuildMap(room, parameters);
    }

    private void BuildMap(Room room, MapGenerationParameters parameters)
    {
        tilemap.Resize(room.Size * BaseTilemapSize);
        buffer = new TileBuffer(tilemap.MapWidth, tilemap.MapHeight);

        FillRandom(parameters.RandomFillAmount, parameters.WallTile, parameters.NothingTile);

        MakeBorder(parameters.BorderSize, parameters.WallTile);
        SmoothBorder(parameters.BorderSize, parameters.BorderGradientSize, parameters.BorderGradientFillAmount,
            parameters.WallTile);

        for (int i = 0; i < parameters.SmoothIterations; i++)
        {
            Smooth(parameters.WallTile, parameters.NothingTile);
        }

        foreach (var processor in processors)
        {
            processor.ProcessMap(buffer, room);
        }

        tilemap.CopyFrom(buffer);
    }

    private void FillRandom(float fillAmount, Tile positiveTile, Tile negativeTile)
    {
        foreach (Coord coord in buffer.Coords)
        {
            buffer[coord] = random.NextFloat() < fillAmount ? positiveTile : negativeTile;
        }
    }

    private void MakeBorder(int size, Tile borderTile)
    {
        foreach (Coord coord in buffer.Coords)
        {
            if (IsInRing(coord, 0, size))
                buffer[coord] = borderTile;
        }
    }

    private bool IsInRing(Coord coord, int outerMargin, int size)
    {
        return (coord.X >= outerMargin && coord.X < outerMargin + size) || (coord.X > buffer.Width - outerMargin - size && coord.X <= buffer.Width - outerMargin)
            || (coord.Y >= outerMargin && coord.Y < outerMargin + size) || (coord.Y > buffer.Height - outerMargin - size && coord.Y <= buffer.Height - outerMargin);
    }

    private void SmoothBorder(int borderSize, int gradientSize, float fillPercent, Tile tile)
    {
        foreach (Coord coord in buffer.Coords)
        {
            if (IsInRing(coord, borderSize, gradientSize))
            {
                if (random.NextFloat() < fillPercent)
                    buffer[coord] = tile;
            }
        }
    }

    private void Smooth(Tile positiveTile, Tile negativeTile)
    {
        // TODO: Use double buffering if this ends up too slow
        buffer.CopyTo(out var newMap);

        for (int y = 0; y < tilemap.MapHeight; y++)
        {
            for (int x = 0; x < tilemap.MapWidth; x++)
            {
                Coord coord = new(x, y);
                int neighborWalls = CountNeighbors(coord, "Wall");

                if (neighborWalls > 4)
                    newMap[x, y] = positiveTile;
                else if (neighborWalls < 4)
                    newMap[x, y] = negativeTile;
            }
        }
        
        buffer.CopyFrom(newMap);
    }

    private int CountNeighbors(Coord coord, string tag)
    {
        int count = 0;
        for (int y = coord.Y - 1; y <= coord.Y + 1; y++)
        {
            for (int x = coord.X - 1; x <= coord.X + 1; x++)
            {
                if (x == y) continue;
                
                Coord neighborCoord = new(x, y);
                if (!buffer.IsInBounds(neighborCoord)) 
                    count++;
                else if (buffer[neighborCoord].Tags.Contains(tag))
                    count++;
            }
        }

        return count;
    }
}