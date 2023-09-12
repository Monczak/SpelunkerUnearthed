using System;
using SpelunkerUnearthed.Engine;
using SpelunkerUnearthed.Engine.Components;
using SpelunkerUnearthed.Engine.Logging;
using SpelunkerUnearthed.Engine.Services;
using SpelunkerUnearthed.Engine.Tiles;

namespace SpelunkerUnearthed.Scripts.MapGeneration;

public class MapGenerator : Component
{
    private Tilemap tilemap;

    public override void OnAttach()
    {
        tilemap = GetComponent<Tilemap>();
    }

    public void GenerateMap(MapGenerationParameters parameters)
    {
        FillRandom(parameters.RandomFillAmount, parameters.WallTile, parameters.NothingTile);

        for (int i = 0; i < parameters.SmoothIterations; i++)
        {
            Smooth(parameters.WallTile, parameters.NothingTile);
        }
        
    }

    private void FillRandom(float fillAmount, Tile positiveTile, Tile negativeTile)
    {
        Random random = new Random();

        foreach (Coord coord in tilemap.Coords)
        {
            tilemap[coord] = random.NextSingle() < fillAmount ? positiveTile : negativeTile;
        }
    }

    private void Smooth(Tile positiveTile, Tile negativeTile)
    {
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
                if (tilemap.IsInBounds(neighborCoord)
                    && tilemap[neighborCoord].Tags.Contains(tag))
                    count++;
            }
        }

        return count;
    }
}