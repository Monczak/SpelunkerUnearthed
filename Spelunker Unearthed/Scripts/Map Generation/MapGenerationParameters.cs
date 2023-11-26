using MariEngine.Tiles;
using SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;

namespace SpelunkerUnearthed.Scripts.MapGeneration;

public struct MapGenerationParameters
{
    public int Seed { get; init; }  // TODO: No longer necessary?
    
    public Tile NothingTile { get; init; }
    
    public float RandomFillAmount { get; init; }
    
    public int BorderSize { get; init; }
    public int BorderGradientSize { get; init; }
    public float BorderGradientFillAmount { get; init; }

    public int SmoothIterations { get; init; }
}
