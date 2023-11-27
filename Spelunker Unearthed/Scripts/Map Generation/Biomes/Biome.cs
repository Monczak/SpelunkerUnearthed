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
    
    public TileProvider WallTileProvider { get; private set; }
    public TileProvider GroundTileProvider { get; private set; }
    
    protected override void BuildFromData(BiomeData data)
    {
        Name = data.Name;
        Color = ColorUtils.FromHex(data.Color);
        WallTileProvider = ParameterProviderRegistry.GetTileProvider(data.WallProvider);
        GroundTileProvider = ParameterProviderRegistry.GetTileProvider(data.GroundProvider);
    }
}