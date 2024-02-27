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

public class RoomMapGenerator(IEnumerable<IRoomMapProcessor> processors, int levelDepth, Room room, BiomeMap biomeMap, RoomMapGenerationParameters parameters, int baseTilemapSize = 16)
{
    private TileBuffer wallBuffer;
    private TileBuffer groundBuffer;

    private PositionBasedRandom random;
    
    public (TileBuffer walls, TileBuffer ground) GenerateRoomMap(Coord pastePosition)
    {
        random = ServiceRegistry.Get<RandomProvider>().RequestDeterministic(Constants.MapGenRng);

        return BuildRoomMap(pastePosition);
    }

    private (TileBuffer walls, TileBuffer ground) BuildRoomMap(Coord pastePosition)
    {
        wallBuffer = new TileBuffer(room.Size * baseTilemapSize);
        groundBuffer = new TileBuffer(room.Size * baseTilemapSize);

        MakeGround(pastePosition);

        FillRandom(pastePosition, parameters.NothingTile);

        MakeBorder(parameters.BorderSize, pastePosition);
        SmoothBorder(parameters.BorderSize, parameters.BorderGradientSize, parameters.BorderGradientFillAmount,
            pastePosition);
        
        Smooth(pastePosition, parameters.NothingTile);

        foreach (var processor in processors)
        {
            processor.ProcessRoomMap(wallBuffer, room);
        }

        return (wallBuffer, groundBuffer);
    }

    private void MakeGround(Coord basePos)
    {
        foreach (Coord coord in groundBuffer.Coords)
        {
            groundBuffer[coord] = biomeMap.GetGround(coord + basePos, levelDepth);
        }
    }

    private void FillRandom(Coord basePos, Tile negativeTile)
    {
        foreach (Coord coord in wallBuffer.Coords)
        {
            Coord worldPos = coord + basePos;
            var x = random.WithPosition(worldPos).NextFloat();
            wallBuffer[coord] = x < biomeMap.GetRandomFillAmount(worldPos, levelDepth) 
                ? biomeMap.GetWall(worldPos, levelDepth) 
                : negativeTile;
        }
    }

    private void MakeBorder(int size, Coord basePos)
    {
        foreach (Coord coord in wallBuffer.Coords)
        {
            if (IsInRing(coord, 0, size))
                wallBuffer[coord] = biomeMap.GetWall(coord + basePos, levelDepth);
        }
    }

    private bool IsInRing(Coord coord, int outerMargin, int size)
    {
        return (coord.X >= outerMargin && coord.X < outerMargin + size) || (coord.X > wallBuffer.Width - outerMargin - size && coord.X <= wallBuffer.Width - outerMargin)
            || (coord.Y >= outerMargin && coord.Y < outerMargin + size) || (coord.Y > wallBuffer.Height - outerMargin - size && coord.Y <= wallBuffer.Height - outerMargin);
    }

    private void SmoothBorder(int borderSize, int gradientSize, float fillPercent, Coord basePos)
    {
        foreach (Coord coord in wallBuffer.Coords)
        {
            if (IsInRing(coord, borderSize, gradientSize))
            {
                if (random.WithPosition(coord).NextFloat() < fillPercent)
                    wallBuffer[coord] = biomeMap.GetWall(coord + basePos, levelDepth);
            }
        }
    }

    private void Smooth(Coord basePos, Tile negativeTile)
    {
        int maxIterationCount = 0;
        foreach (Coord coord in wallBuffer.Coords)
        {
            int iterCount = biomeMap.GetSmoothIterations(coord + basePos, levelDepth);
            if (iterCount > maxIterationCount)
                maxIterationCount = iterCount;
        }

        for (int i = 0; i < maxIterationCount; i++)
        {
            // TODO: Use double buffering if this ends up too slow
            wallBuffer.CopyTo(out var newMap);
            foreach (Coord coord in newMap.Coords)
            {
                if (i >= biomeMap.GetSmoothIterations(coord + basePos, levelDepth))
                    continue;

                int neighborWalls = CountNeighbors(coord, "Wall");

                if (neighborWalls > 4)
                    newMap[coord] = biomeMap.GetWall(coord + basePos, levelDepth);
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