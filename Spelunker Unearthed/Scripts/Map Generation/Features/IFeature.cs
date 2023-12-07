using MariEngine;
using MariEngine.Tiles;

namespace SpelunkerUnearthed.Scripts.MapGeneration.Features;

public interface IFeature
{
    CoordBounds Bounds { get; }

    TileBuffer Generate(Tilemap tilemap);
}