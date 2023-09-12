using SpelunkerUnearthed.Engine.Tiles;

namespace SpelunkerUnearthed.Scripts.MapGeneration;

public struct MapGenerationParameters
{
    public Tile WallTile { get; init; }
    public Tile NothingTile { get; init; }
    
    public float RandomFillAmount { get; init; }
    public int SmoothIterations { get; init; }
}