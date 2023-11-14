using MariEngine.Loading;

namespace SpelunkerUnearthed.Scripts.MapGeneration.Biomes;

public class Biome : Resource<BiomeData>
{
    public string Name { get; private set; }
    
    protected override void BuildFromData(BiomeData data)
    {
        Name = data.Name;
    }
}