using SpelunkerUnearthed.Engine.Tiles;

namespace SpelunkerUnearthed.Scripts.MapGeneration;

public struct MapGenerationParameters
{
    public int Seed { get; init; }
    
    public Tile WallTile { get; init; }
    public Tile NothingTile { get; init; }
    
    public float RandomFillAmount { get; init; }
    
    public int BorderSize { get; init; }
    public int BorderGradientSize { get; init; }
    public float BorderGradientFillAmount { get; init; }

    public int SmoothIterations { get; init; }
}
