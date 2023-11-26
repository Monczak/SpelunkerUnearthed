using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MariEngine;
using MariEngine.Components;
using MariEngine.Logging;
using MariEngine.Services;
using MariEngine.Tiles;
using MariEngine.Utils;
using SpelunkerUnearthed.Scripts.MapGeneration.Biomes;
using SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;
using SpelunkerUnearthed.Scripts.MapGeneration.MapProcessors;
using SpelunkerUnearthed.Scripts.MapGeneration.TileProviders;
using Random = MariEngine.Utils.Random;

namespace SpelunkerUnearthed.Scripts.MapGeneration;

public class MapGenerator : Component
{
    private Tilemap tilemap;
    private TileBuffer buffer;

    private Random random;

    private List<MapProcessor> processors = new();

    protected override void OnAttach()
    {
        tilemap = GetComponent<Tilemap>();
    }

    public void AddProcessor<T>(T processor) where T : MapProcessor
    {
        processors.Add(processor);
    }   
    
    public void GenerateRoomMap(Room room, MapGenerationParameters parameters, Coord pastePosition, BiomeMap biomeMap, int baseTilemapSize = 16)
    {
        random = ServiceRegistry.Get<RandomProvider>().Request(Constants.MapGenRng);

        BuildRoomMap(room, parameters, pastePosition, biomeMap, baseTilemapSize);
    }

    private void BuildRoomMap(Room room, MapGenerationParameters parameters, Coord pastePosition, BiomeMap biomeMap, int baseTilemapSize)
    {
        buffer = new TileBuffer(room.Size * baseTilemapSize);

        FillRandom(parameters.RandomFillAmount, pastePosition, biomeMap, parameters.NothingTile);

        MakeBorder(parameters.BorderSize, pastePosition, biomeMap);
        SmoothBorder(parameters.BorderSize, parameters.BorderGradientSize, parameters.BorderGradientFillAmount,
            pastePosition, biomeMap);

        for (int i = 0; i < parameters.SmoothIterations; i++)
        {
            Smooth(pastePosition, biomeMap, parameters.NothingTile);
        }

        foreach (var processor in processors)
        {
            processor.ProcessMap(buffer, room);
        }
        
        tilemap.PasteAt(buffer, pastePosition, Tilemap.BaseLayer);  // TODO: Add support for more layers
    }

    private void FillRandom(float fillAmount, Coord basePos, BiomeMap biomeMap, Tile negativeTile)
    {
        foreach (Coord coord in buffer.Coords)
        {
            buffer[coord] = random.NextFloat() < fillAmount 
                ? biomeMap.GetWall(coord + basePos) 
                : negativeTile;
        }
    }

    private void MakeBorder(int size, Coord basePos, BiomeMap biomeMap)
    {
        foreach (Coord coord in buffer.Coords)
        {
            if (IsInRing(coord, 0, size))
                buffer[coord] = biomeMap.GetWall(coord + basePos);
        }
    }

    private bool IsInRing(Coord coord, int outerMargin, int size)
    {
        return (coord.X >= outerMargin && coord.X < outerMargin + size) || (coord.X > buffer.Width - outerMargin - size && coord.X <= buffer.Width - outerMargin)
            || (coord.Y >= outerMargin && coord.Y < outerMargin + size) || (coord.Y > buffer.Height - outerMargin - size && coord.Y <= buffer.Height - outerMargin);
    }

    private void SmoothBorder(int borderSize, int gradientSize, float fillPercent, Coord basePos, BiomeMap biomeMap)
    {
        foreach (Coord coord in buffer.Coords)
        {
            if (IsInRing(coord, borderSize, gradientSize))
            {
                if (random.NextFloat() < fillPercent)
                    buffer[coord] = biomeMap.GetWall(coord + basePos);
            }
        }
    }

    private void Smooth(Coord basePos, BiomeMap biomeMap,  Tile negativeTile)
    {
        // TODO: Use double buffering if this ends up too slow
        buffer.CopyTo(out var newMap);

        foreach (Coord coord in newMap.Coords)
        {
            int neighborWalls = CountNeighbors(coord, "Wall");

            if (neighborWalls > 4)
                newMap[coord] = biomeMap.GetWall(coord + basePos);
            else if (neighborWalls < 4)
                newMap[coord] = negativeTile;
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