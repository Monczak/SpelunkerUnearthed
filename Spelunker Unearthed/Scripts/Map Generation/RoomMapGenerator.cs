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
using SpelunkerUnearthed.Scripts.MapGeneration.ParameterProviders;
using Random = MariEngine.Utils.Random;

namespace SpelunkerUnearthed.Scripts.MapGeneration;

public class RoomMapGenerator(IEnumerable<IRoomMapProcessor> processors)
{
    private TileBuffer wallBuffer;
    private TileBuffer groundBuffer;

    private Random random;
    
    public (TileBuffer walls, TileBuffer ground) GenerateRoomMap(Room room, RoomMapGenerationParameters parameters, Coord pastePosition, BiomeMap biomeMap, int baseTilemapSize = 16)
    {
        random = ServiceRegistry.Get<RandomProvider>().Request(Constants.MapGenRng);

        return BuildRoomMap(room, parameters, pastePosition, biomeMap, baseTilemapSize);
    }

    private (TileBuffer walls, TileBuffer ground) BuildRoomMap(Room room, RoomMapGenerationParameters parameters, Coord pastePosition, BiomeMap biomeMap, int baseTilemapSize)
    {
        wallBuffer = new TileBuffer(room.Size * baseTilemapSize);
        groundBuffer = new TileBuffer(room.Size * baseTilemapSize);

        MakeGround(pastePosition, biomeMap);

        FillRandom(pastePosition, biomeMap, parameters.NothingTile);

        MakeBorder(parameters.BorderSize, pastePosition, biomeMap);
        SmoothBorder(parameters.BorderSize, parameters.BorderGradientSize, parameters.BorderGradientFillAmount,
            pastePosition, biomeMap);
        
        Smooth(pastePosition, biomeMap, parameters.NothingTile);

        foreach (var processor in processors)
        {
            processor.ProcessRoomMap(wallBuffer, room);
        }

        return (wallBuffer, groundBuffer);
    }

    private void MakeGround(Coord basePos, BiomeMap biomeMap)
    {
        foreach (Coord coord in groundBuffer.Coords)
        {
            groundBuffer[coord] = biomeMap.GetGround(coord + basePos);
        }
    }

    private void FillRandom(Coord basePos, BiomeMap biomeMap, Tile negativeTile)
    {
        foreach (Coord coord in wallBuffer.Coords)
        {
            Coord worldPos = coord + basePos;
            wallBuffer[coord] = random.NextFloat() < biomeMap.GetRandomFillAmount(worldPos) 
                ? biomeMap.GetWall(worldPos) 
                : negativeTile;
        }
    }

    private void MakeBorder(int size, Coord basePos, BiomeMap biomeMap)
    {
        foreach (Coord coord in wallBuffer.Coords)
        {
            if (IsInRing(coord, 0, size))
                wallBuffer[coord] = biomeMap.GetWall(coord + basePos);
        }
    }

    private bool IsInRing(Coord coord, int outerMargin, int size)
    {
        return (coord.X >= outerMargin && coord.X < outerMargin + size) || (coord.X > wallBuffer.Width - outerMargin - size && coord.X <= wallBuffer.Width - outerMargin)
            || (coord.Y >= outerMargin && coord.Y < outerMargin + size) || (coord.Y > wallBuffer.Height - outerMargin - size && coord.Y <= wallBuffer.Height - outerMargin);
    }

    private void SmoothBorder(int borderSize, int gradientSize, float fillPercent, Coord basePos, BiomeMap biomeMap)
    {
        foreach (Coord coord in wallBuffer.Coords)
        {
            if (IsInRing(coord, borderSize, gradientSize))
            {
                if (random.NextFloat() < fillPercent)
                    wallBuffer[coord] = biomeMap.GetWall(coord + basePos);
            }
        }
    }

    private void Smooth(Coord basePos, BiomeMap biomeMap, Tile negativeTile)
    {
        int maxIterationCount = 0;
        foreach (Coord coord in wallBuffer.Coords)
        {
            int iterCount = biomeMap.GetSmoothIterations(coord + basePos);
            if (iterCount > maxIterationCount)
                maxIterationCount = iterCount;
        }

        for (int i = 0; i < maxIterationCount; i++)
        {
            // TODO: Use double buffering if this ends up too slow
            wallBuffer.CopyTo(out var newMap);
            foreach (Coord coord in newMap.Coords)
            {
                if (i >= biomeMap.GetSmoothIterations(coord + basePos))
                    continue;

                int neighborWalls = CountNeighbors(coord, "Wall");

                if (neighborWalls > 4)
                    newMap[coord] = biomeMap.GetWall(coord + basePos);
                else if (neighborWalls < 4)
                    newMap[coord] = negativeTile;
            }

            wallBuffer.CopyFrom(newMap);
        }

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
                if (!wallBuffer.IsInBounds(neighborCoord)) 
                    count++;
                else if (Tags.HasTag(wallBuffer[neighborCoord].Tags, tag))
                    count++;
            }
        }

        return count;
    }
}