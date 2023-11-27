using System;
using MariEngine;
using MariEngine.Services;
using MariEngine.Tiles;

namespace SpelunkerUnearthed.Scripts.MapGeneration.TileProviders;


[ParameterProviderName("Basic")]
public class BasicTileProvider : TileProvider
{
    private Tile tile;

    protected override void BuildFromData(ParameterProviderData data)
    {
        tile = ServiceRegistry.Get<TileLoader>().Get(data.Options["Tile"] as string);
    }

    public override Tile Get(Coord worldPos)
    {
        return tile;
    }
}