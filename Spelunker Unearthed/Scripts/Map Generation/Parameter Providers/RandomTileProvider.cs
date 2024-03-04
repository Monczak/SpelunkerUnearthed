using System;
using System.Collections.Generic;
using MariEngine;
using MariEngine.Loading;
using MariEngine.Services;
using MariEngine.Tiles;

namespace SpelunkerUnearthed.Scripts.MapGeneration.ParameterProviders;


[ParameterProviderName("Random")]
public class RandomTileProvider : TileProvider
{
    private List<(Tile, float)> tiles;

    protected override void BuildFromData(ParameterProviderData data)
    {
        tiles = [];
        if (data.Options["Tiles"] is not Dictionary<object, object> tileWeights)
            throw new ResourceLoadingException("Could not load tile weights.");

        foreach (var pair in tileWeights)
        {
            var tileId = pair.Key as string;
            if (pair.Value is not string weightStr || !float.TryParse(weightStr, out float weight))
                throw new ResourceLoadingException($"Could not parse weight for tile {tileId}.");
            
            tiles.Add((ServiceRegistry.Get<TileLoader>().Get(tileId), weight));
        }
    }

    public override Tile Get(Coord worldPos)
    {
        return ServiceRegistry.Get<RandomProvider>().RequestPositionBased(Constants.MapGenRng)
            .WithPosition(worldPos)
            .PickWeighted(tiles, out _);
    }
}