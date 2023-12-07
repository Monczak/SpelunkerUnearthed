using MariEngine;
using MariEngine.Tiles;

namespace SpelunkerUnearthed.Scripts.MapGeneration.Features;

public abstract class ProceduralFeature : IFeature
{
    public abstract CoordBounds Bounds { get; protected set; }

    public TileBuffer Generate(Tilemap tilemap) => GenerateFeature(tilemap);

    protected abstract TileBuffer GenerateFeature(Tilemap tilemap);
}