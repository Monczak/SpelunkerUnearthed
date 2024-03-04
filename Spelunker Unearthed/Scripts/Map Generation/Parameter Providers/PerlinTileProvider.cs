using System;
using System.Collections.Generic;
using System.Linq;
using MariEngine;
using MariEngine.Loading;
using MariEngine.Logging;
using MariEngine.Services;
using MariEngine.Tiles;
using Microsoft.Xna.Framework;

namespace SpelunkerUnearthed.Scripts.MapGeneration.ParameterProviders;

public class PerlinTileProvider : TileProvider
{
    private SortedList<float, Tile> tileThresholds;
    private Vector2 frequency;
    private string rngId;
    
    public override Tile Get(Coord worldPos)
    {
        float noise = ServiceRegistry.Get<RandomProvider>().RequestPositionBased(Constants.MapGenRng)
            .WithPosition(worldPos)
            .Perlin01((Vector2)worldPos * frequency);
        foreach (var (threshold, tile) in tileThresholds)
        {
            if (noise < threshold)
                return tile;
        }

        return tileThresholds.Last().Value;
    }

    protected override void BuildFromData(ParameterProviderData data)
    {
        tileThresholds = new SortedList<float, Tile>();
        if (data.Options["Tiles"] is not Dictionary<object, object> thresholds)
            throw new ResourceLoadingException("Could not load tile thresholds.");

        foreach (var pair in thresholds)
        {
            var tileId = pair.Key as string;
            if (pair.Value is not string thresholdStr || !float.TryParse(thresholdStr, out float threshold))
                throw new ResourceLoadingException($"Could not parse threshold for tile {tileId}.");
            
            tileThresholds.Add(threshold, ServiceRegistry.Get<TileLoader>().Get(tileId));
        }

        if (!ParseUtils.TryParseVector2(data.Options, "Frequency", out frequency))
            throw new ResourceLoadingException("Could not parse Perlin noise frequency.");

        ParseUtils.GetStringOrDefault(data.Options, "Rng", out rngId, required: false,
            defaultValue: Constants.GeneralPurposeRng);
    }
}