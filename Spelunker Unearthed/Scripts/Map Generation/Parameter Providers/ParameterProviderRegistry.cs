using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MariEngine.Loading;
using MariEngine.Tiles;

namespace SpelunkerUnearthed.Scripts.MapGeneration.TileProviders;

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
}