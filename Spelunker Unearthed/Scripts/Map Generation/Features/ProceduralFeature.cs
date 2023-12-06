using MariEngine;
using MariEngine.Tiles;

namespace SpelunkerUnearthed.Scripts.MapGeneration.Features;

public abstract class ProceduralFeature : IFeature
{
    public abstract string Name { get; set; }
    public abstract CoordBounds Bounds { get; set; }

    public TileBuffer Generate() => GenerateFeature();

    protected abstract TileBuffer GenerateFeature();
}