using System.Collections.Generic;
using System.Linq;
using MariEngine.Loading;
using MariEngine.Tiles;
using YamlDotNet.Serialization;

namespace MariEngine.Services;

public class TileLoader : ResourceLoaderService<Tile, TileData>
{
    protected override string ContentPath => ContentPaths.Tiles;

    private string GetCharacterTileId(char c) => $"Character_{c}";

    private IEnumerable<Tile> GetSpecialTiles()
    {
        yield return ResourceBuilder.Build<Tile, TileData>("Nothing", new TileData
        {
            ForegroundColor = "#000000",
            BackgroundColor = "#000000",
            Character = ' ',
            Tags = null,
            Behaviors = null,
            LightAttenuation = 0,
        });
    }

    private IEnumerable<Tile> GetUiTextTiles()
    {
        return "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz 1234567890!@#$%^&*()-=_+[]{};':\",.<>/?\\|"
            .Select(character => ResourceBuilder.Build<Tile, TileData>(GetCharacterTileId(character), new TileData
            {
                ForegroundColor = "#ffffff",
                BackgroundColor = "#00000000",
                Character = character,
                Tags = null,
                Behaviors = null,
                LightAttenuation = 0,
                Type = "Ui"
            }));
    }

    public override void LoadContent(INamingConvention convention = null)
    {
        base.LoadContent(convention);

        foreach (var tile in GetSpecialTiles()) Content.Add(tile.Id, tile);
        foreach (var tile in GetUiTextTiles()) Content.Add(tile.Id, tile);
    }

    public Tile GetCharacter(char c) => Get(GetCharacterTileId(c));
}