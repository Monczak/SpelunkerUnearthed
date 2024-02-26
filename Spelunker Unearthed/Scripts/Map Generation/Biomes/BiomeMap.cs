using MariEngine;
using MariEngine.Tiles;

namespace SpelunkerUnearthed.Scripts.MapGeneration.Biomes;

public class BiomeMap(IBiomeProvider biomeProvider)
{
    public void SetBiomeProvider(IBiomeProvider provider)
    {
        biomeProvider = provider;
    }

    // TODO: Cache results nicely
    public Biome GetBiome(Coord worldPos, int level)
    {
        return biomeProvider.GetBiome(worldPos, level);
    }

    public Tile GetWall(Coord worldPos, int level)
    {
        return GetBiome(worldPos, level).WallTileProvider.Get(worldPos);
    }

    public Tile GetGround(Coord worldPos, int level)
    {
        return GetBiome(worldPos, level).GroundTileProvider.Get(worldPos);
    }

    public float GetRandomFillAmount(Coord worldPos, int level)
    {
        return GetBiome(worldPos, level).RandomFillAmountProvider.Get(worldPos);
    }
    
    public int GetSmoothIterations(Coord worldPos, int level)
    {
        return (int)GetBiome(worldPos, level).SmoothIterationsProvider.Get(worldPos);
    }
}