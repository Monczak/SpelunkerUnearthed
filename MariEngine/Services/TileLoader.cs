using System;
using System.Collections.Generic;
using System.IO;
using MariEngine.Exceptions;
using MariEngine.Logging;
using MariEngine.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MariEngine.Services;

public class TileLoader : Service
{
    private readonly string tilePath = ContentPaths.Tiles;

    public Dictionary<string, Tile> Tiles { get; private set; }

    public Dictionary<string, Tile> LoadTiles()
    {
        Tiles = new Dictionary<string, Tile>
        {
            ["Nothing"] = new("Nothing", new TileData
            {
                ForegroundColor = "#000000", 
                BackgroundColor = "#000000", 
                Character = ' ', 
                Tags = null, 
                Behaviors = null,
                LightAttenuation = 0,
            })
        };

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        
        foreach (var file in Directory.GetFiles(tilePath))
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

    public Tile GetTile(string id) => Tiles[id];
}