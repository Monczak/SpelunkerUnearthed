using MariEngine;
using MariEngine.Services;

namespace SpelunkerUnearthed.Scripts.MapGeneration.Biomes;

public class BiomeLoader : LoaderService<Biome, BiomeData>
{
    protected override string ContentPath => ContentPaths.Biomes;
}