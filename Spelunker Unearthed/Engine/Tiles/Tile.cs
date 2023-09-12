using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SpelunkerUnearthed.Engine.Utils;

namespace SpelunkerUnearthed.Engine.Tiles;

public class Tile
{
    public string Id { get; }
    
    public Color ForegroundColor { get; }
    public Color BackgroundColor { get; }
    public char Character { get; }
    
    public HashSet<string> Tags { get; }
    
    public Tilemap OwnerTilemap { get; set; }
    public HashSet<TileBehavior> Behaviors { get; }
    
    public Tile(string id, TileData data) : this(
        id,
        ColorUtils.FromHex(data.ForegroundColor), 
        ColorUtils.FromHex(data.BackgroundColor), 
        data.Character, data.Tags)
    {
        
    }

    public Tile(string id, Color foregroundColor, Color backgroundColor, char character, string[] tags)
    {
        Id = id;
        ForegroundColor = foregroundColor;
        BackgroundColor = backgroundColor;
        Character = character;

        Tags = tags is null ? new HashSet<string>() : new HashSet<string>(tags);
        
        Behaviors = new HashSet<TileBehavior>();
    }

    public Tile(Tile tile)
    {
        Id = tile.Id;
        ForegroundColor = tile.ForegroundColor;
        BackgroundColor = tile.BackgroundColor;
        Character = tile.Character;
        
        Tags = tile.Tags is null ? new HashSet<string>() : new HashSet<string>(tile.Tags);

        Behaviors = new HashSet<TileBehavior>(tile.Behaviors);
    }
}