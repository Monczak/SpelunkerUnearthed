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
    public Biome GetBiome(Coord worldPos)
    {
        return biomeProvider.GetBiome(worldPos);
    }

    public Tile GetWall(Coord worldPos)
    {
        return GetBiome(worldPos).WallTileProvider.Get(worldPos);
    }

    public Tile GetGround(Coord worldPos)
    {
        return GetBiome(worldPos).GroundTileProvider.Get(worldPos);
    }

    public float GetRandomFillAmount(Coord worldPos)
    {
        return GetBiome(worldPos).RandomFillAmountProvider.Get(worldPos);
    }
    
    public int GetSmoothIterations(Coord worldPos)
    {
        return (int)GetBiome(worldPos).SmoothIterationsProvider.Get(worldPos);
    }
}