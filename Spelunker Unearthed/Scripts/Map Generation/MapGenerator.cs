using System;
using Microsoft.Xna.Framework;
using MariEngine;
using MariEngine.Components;
using MariEngine.Logging;
using MariEngine.Services;
using MariEngine.Tiles;
using MariEngine.Utils;

namespace SpelunkerUnearthed.Scripts.MapGeneration;

public class MapGenerator : Component
{
    private Tilemap tilemap;

    private Random random;

    public override void OnAttach()
    {
        tilemap = GetComponent<Tilemap>();
    }

    public void GenerateMap(MapGenerationParameters parameters)
    {
        random = new Random(parameters.Seed);
        
        FillRandom(parameters.RandomFillAmount, parameters.WallTile, parameters.NothingTile);
        
        MakeBorder(parameters.BorderSize, parameters.WallTile);
        SmoothBorder(parameters.BorderSize, parameters.BorderGradientSize, parameters.BorderGradientFillAmount, parameters.WallTile);

        for (int i = 0; i < parameters.SmoothIterations; i++)
        {
            Smooth(parameters.WallTile, parameters.NothingTile);
        }
    }

    private void FillRandom(float fillAmount, Tile positiveTile, Tile negativeTile)
    {
        foreach (Coord coord in tilemap.Coords)
        {
            tilemap.Place(random.NextSingle() < fillAmount ? positiveTile : negativeTile, coord);
        }
    }

    private void MakeBorder(int size, Tile borderTile)
    {
        foreach (Coord coord in tilemap.Coords)
        {
            if (IsInRing(coord, 0, size))
                tilemap.Place(borderTile, coord);
        }
    }

    private bool IsInRing(Coord coord, int outerMargin, int size)
    {
        return (coord.X >= outerMargin && coord.X < outerMargin + size) || (coord.X > tilemap.MapWidth - outerMargin - size && coord.X <= tilemap.MapWidth - outerMargin)
            || (coord.Y >= outerMargin && coord.Y < outerMargin + size) || (coord.Y > tilemap.MapHeight - outerMargin - size && coord.Y <= tilemap.MapHeight - outerMargin);
    }

    private void SmoothBorder(int borderSize, int gradientSize, float fillPercent, Tile tile)
    {
        foreach (Coord coord in tilemap.Coords)
        {
            if (IsInRing(coord, borderSize, gradientSize))
            {
                if (random.NextSingle() < fillPercent)
                    tilemap.Place(tile, coord);
            }
        }
    }

    private void Smooth(Tile positiveTile, Tile negativeTile)
    {
        // TODO: Use double buffering if this ends up too slow
        tilemap.CopyTo(out var newMap);

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
        
        tilemap.CopyFrom(newMap);
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
                if (!tilemap.IsInBounds(neighborCoord)) 
                    count++;
                else if (tilemap[neighborCoord].Tags.Contains(tag))
                    count++;
            }
        }

        return count;
    }
}