using System.Collections.Generic;
using MariEngine.Loading;
using MariEngine.Tiles;

namespace MariEngine.Services;

public class TileLoader : LoaderService<Tile, TileData>
{
    protected override string ContentPath => ContentPaths.Tiles;

    public new Dictionary<string, Tile> LoadContent()
    {
        base.LoadContent();
        
        Tile nothingTile = LoadableObjectBuilder.Build<Tile, TileData>("Nothing", new TileData
        {
            ForegroundColor = "#000000",
            BackgroundColor = "#000000",
            Character = ' ',
            Tags = null,
            Behaviors = null,
            LightAttenuation = 0,
        });
        Content.Add(nothingTile.Id, nothingTile);

        return Content;
    }
}