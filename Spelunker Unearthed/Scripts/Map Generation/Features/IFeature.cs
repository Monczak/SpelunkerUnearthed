using MariEngine;
using MariEngine.Tiles;

namespace SpelunkerUnearthed.Scripts.MapGeneration.Features;

public interface IFeature
{
    string Name { get; protected set; }
    CoordBounds Bounds { get; }

    TileBuffer Generate();
}