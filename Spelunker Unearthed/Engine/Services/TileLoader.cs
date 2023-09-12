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

    public Dictionary<string, Tile> Tiles { get; private set; }

    public TileLoader()
    {
        Logger.Log("Initialized tile loader");
    }

    public Dictionary<string, Tile> LoadTiles()
    {
        Tiles = new Dictionary<string, Tile>
        {
            ["Nothing"] = new("Nothing", Color.Black, Color.Black, ' ', Array.Empty<string>())
        };

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        
        foreach (var file in Directory.GetFiles(TilePath))
        {
            string tileId = Path.GetFileNameWithoutExtension(file);
            try
            {
                Tile tile = new(tileId, deserializer.Deserialize<TileData>(File.ReadAllText(file)));
                if (Tiles.ContainsKey(tileId))
                {
                    throw new TileLoadingException($"Tile with ID {tileId} already exists.");
                }
                Tiles[tileId] = tile;
            }
            catch (Exception e)
            {
                Logger.LogError($"Could not load tile {tileId}: {e.GetType().Name}: {e.Message}");
            }
        }
        
        Logger.Log($"Loaded {Tiles.Count} tiles");

        return Tiles;
    }

    public Tile GetTile(string id) => new(Tiles[id]);
}