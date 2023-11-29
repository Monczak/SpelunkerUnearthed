using System;
using MariEngine.Loading;

namespace SpelunkerUnearthed.Scripts.MapGeneration.ParameterProviders;

public static class ParameterProviderRegistry
{
    // TODO: Get all parameter providers using reflection
    public static TileProvider GetTileProvider(ParameterProviderData data)
    {
        return data.Type switch
        {
            "Basic" => ResourceBuilder.Build<BasicTileProvider, ParameterProviderData>("BasicTileProvider", data),
            "Random" => ResourceBuilder.Build<RandomTileProvider, ParameterProviderData>("RandomTileProvider", data),
            "Perlin" => ResourceBuilder.Build<PerlinTileProvider, ParameterProviderData>("PerlinTileProvider", data),
            _ => throw new ArgumentException($"Unknown tile provider type: {data.Type}")
        };
    }

    public static NumberProvider GetNumberProvider(ParameterProviderData data)
    {
        return data.Type switch
        {
            "Constant" => ResourceBuilder.Build<ConstantNumberProvider, ParameterProviderData>("ConstantNumberProvider", data),
            "Random" => ResourceBuilder.Build<RandomNumberProvider, ParameterProviderData>("RandomNumberProvider", data),
            "Perlin" => ResourceBuilder.Build<PerlinNumberProvider, ParameterProviderData>("PerlinNumberProvider", data),
            _ => throw new ArgumentException($"Unknown number provider type: {data.Type}")
        };
    }
}