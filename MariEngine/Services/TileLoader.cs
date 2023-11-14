using System.Collections.Generic;
using MariEngine.Loading;
using MariEngine.Tiles;
using YamlDotNet.Serialization;

namespace MariEngine.Services;

public class TileLoader : ResourceLoaderService<Tile, TileData>
{
    protected override string ContentPath => ContentPaths.Tiles;

    public override void LoadContent(INamingConvention convention = null)
    {
        base.LoadContent(convention);
        
        Tile nothingTile = ResourceBuilder.Build<Tile, TileData>("Nothing", new TileData
        {
            ForegroundColor = "#000000",
            BackgroundColor = "#000000",
            Character = ' ',
            Tags = null,
            Behaviors = null,
            LightAttenuation = 0,
        });
        Content.Add(nothingTile.Id, nothingTile);
    }
}