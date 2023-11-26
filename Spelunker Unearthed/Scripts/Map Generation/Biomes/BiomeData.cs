using SpelunkerUnearthed.Scripts.MapGeneration.TileProviders;

namespace SpelunkerUnearthed.Scripts.MapGeneration.Biomes;

public struct BiomeData
{
    public string Name { get; private set; }
    public string Color { get; private set; }
    
    public TileProviderData WallProvider { get; private set; }
}