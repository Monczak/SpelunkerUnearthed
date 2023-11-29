using MariEngine;
using MariEngine.Loading;

namespace SpelunkerUnearthed.Scripts.MapGeneration.ParameterProviders;

public abstract class ParameterProvider : Resource<ParameterProviderData>
{
    
}

public abstract class ParameterProvider<T> : ParameterProvider
{
    public abstract T Get(Coord worldPos);
}