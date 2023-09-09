using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using SpelunkerUnearthed.Engine.Exceptions;
using SpelunkerUnearthed.Engine.Logging;
using SpelunkerUnearthed.Engine.Tiles;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SpelunkerUnearthed.Engine.Services;

public class TileLoader : Service
{
    private readonly string TilePath = "Content/Tiles";

    private Dictionary<string, Tile> tiles;

    public TileLoader()
    {
        Logger.Log("Initialized tile loader");
    }

    public void LoadTiles()
    {
        tiles = new Dictionary<string, Tile>
        {
            ["Nothing"] = new(Color.Black, Color.Black, ' ')
        };

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        
        foreach (var file in Directory.GetFiles(TilePath))
        {
            string tileId = Path.GetFileNameWithoutExtension(file);
            try
            {
                Tile tile = new(deserializer.Deserialize<TileData>(File.ReadAllText(file)));
                if (tiles.ContainsKey(tileId))
                {
                    throw new TileLoadingException($"Tile with ID {tileId} already exists.");
                }
                tiles[tileId] = tile;
            }
            catch (Exception e)
            {
                Logger.LogError($"Could not load tile {tileId}: {e.GetType().Name}: {e.Message}");
            }
        }
        
        Logger.Log($"Loaded {tiles.Count} tiles");
    }

    public Tile GetTile(string id) => tiles[id];
}