using System;
using MariEngine.Loading;
using MariEngine.Tiles;
using MariEngine.Utils;
using Microsoft.Xna.Framework;
using SpelunkerUnearthed.Scripts.MapGeneration.TileProviders;

namespace SpelunkerUnearthed.Scripts.MapGeneration.Biomes;

public class Biome : Resource<BiomeData>
{
    public string Name { get; private set; }
    public Color Color { get; private set; }
    
    public ParameterProvider<Tile> WallProvider { get; private set; }
    
    protected override void BuildFromData(BiomeData data)
    {
        Name = data.Name;
        Color = ColorUtils.FromHex(data.Color);
        WallProvider = data.WallProvider.Type switch
        {
            "Basic" => ResourceBuilder.Build<BasicTileProvider, ParameterProviderData>("BasicTileProvider",
                data.WallProvider),
            _ => throw new ArgumentException($"Unknown tile provider type: {data.WallProvider.Type}")
        };
    }
}