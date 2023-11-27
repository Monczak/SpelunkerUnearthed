using MariEngine;
using MariEngine.Loading;

namespace SpelunkerUnearthed.Scripts.MapGeneration.TileProviders;

public abstract class ParameterProvider : Resource<ParameterProviderData>
{
    
}

public abstract class ParameterProvider<T> : ParameterProvider
{
    public abstract T Get(Coord worldPos);
}