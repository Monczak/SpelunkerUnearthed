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
    
    public Biome GetBiome(Coord worldPos)
    {
        Random random = ServiceRegistry.Get<RandomProvider>().Request(Constants.BiomeGenRng);
        
        (int x, int y) = worldPos;
        Vector3 v = new(x, y, 0);
        v += new Vector3(1, 1, 0) * random.Perlin(new Vector2(x, y) * CoordDisplacementFrequency) * CoordDisplacementAmplitude;
        (float noise, int cellHash) = random.Voronoi(v, VoronoiCellSize);

        var biomes = ServiceRegistry.Get<BiomeLoader>().Content.Values.ToList();
        return biomes[cellHash % biomes.Count];
    }
}