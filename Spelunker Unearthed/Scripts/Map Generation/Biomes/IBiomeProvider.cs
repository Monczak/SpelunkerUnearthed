using MariEngine;

namespace SpelunkerUnearthed.Scripts.MapGeneration.Biomes;

public interface IBiomeProvider
{
    Biome GetBiome(Coord worldPos);
}