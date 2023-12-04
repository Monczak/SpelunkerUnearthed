using MariEngine.Tiles;
using SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;

namespace SpelunkerUnearthed.Scripts.MapGeneration;

public struct RoomMapGenerationParameters
{
    public Tile NothingTile { get; init; }
    
    public int BorderSize { get; init; }
    public int BorderGradientSize { get; init; }
    public float BorderGradientFillAmount { get; init; }

    public int SmoothIterations { get; init; }
}
