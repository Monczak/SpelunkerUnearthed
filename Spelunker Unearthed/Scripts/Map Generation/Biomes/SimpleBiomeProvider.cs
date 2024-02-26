using System.Linq;
using MariEngine;
using MariEngine.Services;
using MariEngine.Utils;
using Microsoft.Xna.Framework;

namespace SpelunkerUnearthed.Scripts.MapGeneration.Biomes;

public class SimpleBiomeProvider : IBiomeProvider
{
    public float CoordDisplacementFrequency { get; init; } = 0.03f;
    public float CoordDisplacementAmplitude { get; init; } = 30;
    public float VoronoiCellSize { get; init; } = 60;
    public float LevelDepthNoiseMultiplier { get; init; } = 50;

    public Biome GetBiome(Coord worldPos, int level)
    {
        Random random = ServiceRegistry.Get<RandomProvider>().Request(Constants.BiomeGenRng);

        Vector3 v = new((Vector2)worldPos, -level * LevelDepthNoiseMultiplier);
        v += Vector3.Right * random.Perlin((Vector2)worldPos * CoordDisplacementFrequency) * CoordDisplacementAmplitude;
        v += Vector3.Up * random.Perlin(((Vector2)worldPos + Vector2.One * 1000) * CoordDisplacementFrequency) *
             CoordDisplacementAmplitude;
        (float noise, int cellHash) = random.Voronoi(v, VoronoiCellSize);

        var biomes = ServiceRegistry.Get<BiomeLoader>().Content.Values.ToList();
        return biomes[cellHash % biomes.Count];
    }
}