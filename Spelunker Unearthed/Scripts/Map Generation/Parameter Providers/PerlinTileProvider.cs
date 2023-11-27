using System;
using System.Collections.Generic;
using System.Linq;
using MariEngine;
using MariEngine.Loading;
using MariEngine.Logging;
using MariEngine.Services;
using MariEngine.Tiles;
using Microsoft.Xna.Framework;

namespace SpelunkerUnearthed.Scripts.MapGeneration.TileProviders;

public class PerlinTileProvider : TileProvider
{
    private SortedList<float, Tile> tileThresholds;
    private Vector2 frequency;
    
    public override Tile Get(Coord worldPos)
    {
        float noise = ServiceRegistry.Get<RandomProvider>().Request(Constants.MapGenRng).Perlin01((Vector2)worldPos * frequency * 1.414f);
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

        if (!TryParseFrequency(data.Options["Frequency"] as string, out frequency))
            throw new ResourceLoadingException("Could not parse Perlin noise frequency.");
    }

    private bool TryParseFrequency(string str, out Vector2 frequency)
    {
        frequency = Vector2.Zero;
        
        var freqStr = str?.Split(" ");
        if (freqStr is null)
            return false;

        if (!float.TryParse(freqStr[0], out var frequencyX) || !float.TryParse(freqStr[1], out var frequencyY))
            return false;
        frequency = new Vector2(frequencyX, frequencyY);

        return true;
    }
}