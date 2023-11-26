using MariEngine;
using MariEngine.Loading;
using MariEngine.Tiles;

namespace SpelunkerUnearthed.Scripts.MapGeneration.TileProviders;

public abstract class TileProvider : Resource<TileProviderData>
{
    public abstract Tile GetTile(Coord worldPos);
}