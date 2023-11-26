using MariEngine;
using MariEngine.Loading;

namespace SpelunkerUnearthed.Scripts.MapGeneration.TileProviders;

public abstract class ParameterProvider<T> : Resource<ParameterProviderData>
{
    public abstract T Get(Coord worldPos);
}