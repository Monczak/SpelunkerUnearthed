using MariEngine.Loading;

namespace SpelunkerUnearthed.Scripts.MapGeneration.Biomes;

public class Biome : LoadableObject<BiomeData>
{
    public string Name { get; private set; }
}