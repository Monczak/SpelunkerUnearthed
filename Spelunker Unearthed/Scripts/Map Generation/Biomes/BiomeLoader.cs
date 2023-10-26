using MariEngine;
using MariEngine.Services;
using ContentPaths = SpelunkerUnearthed.Scripts.ContentPaths;

namespace SpelunkerUnearthed.Scripts.MapGeneration.Biomes;

public class BiomeLoader : LoaderService<Biome, BiomeData>
{
    protected override string ContentPath => ContentPaths.Biomes;
}