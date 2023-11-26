using MariEngine;
using MariEngine.Tiles;

namespace SpelunkerUnearthed.Scripts.MapGeneration.Biomes;

public class BiomeMap
{
    private IBiomeProvider biomeProvider;

    public BiomeMap(IBiomeProvider biomeProvider)
    {
        this.biomeProvider = biomeProvider;
    }

    public void SetBiomeProvider(IBiomeProvider provider)
    {
        biomeProvider = provider;
    }

    public Biome GetBiome(Coord worldPos)
    {
        return biomeProvider.GetBiome(worldPos);
    }

    public Tile GetWall(Coord worldPos)
    {
        return GetBiome(worldPos).WallProvider.GetTile(worldPos);
    }
}