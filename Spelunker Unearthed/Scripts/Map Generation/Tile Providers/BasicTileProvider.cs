﻿using System;
using MariEngine;
using MariEngine.Services;
using MariEngine.Tiles;

namespace SpelunkerUnearthed.Scripts.MapGeneration.TileProviders;

public class BasicTileProvider : ParameterProvider<Tile>
{
    private Tile tile;

    protected override void BuildFromData(ParameterProviderData data)
    {
        if (data.Type != "Basic")
            throw new ArgumentException("Tried to initialize BasicTileProvider, but Type was not Basic.");

        tile = ServiceRegistry.Get<TileLoader>().Get(data.Options["Tile"]);
    }

    public override Tile Get(Coord worldPos)
    {
        return new Tile(tile);
    }
}