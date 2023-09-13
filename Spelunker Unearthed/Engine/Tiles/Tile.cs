using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SpelunkerUnearthed.Engine.Collision;
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

    public CollisionGroup CollisionGroup { get; }
    
    public Tile(string id, TileData data) : this(
        id,
        ColorUtils.FromHex(data.ForegroundColor), 
        ColorUtils.FromHex(data.BackgroundColor), 
        data.Character, data.Tags, data.CollisionGroups)
    {
        
    }

    public Tile(string id, Color foregroundColor, Color backgroundColor, char character, string[] tags, string[] collisionGroups)
    {
        Id = id;
        ForegroundColor = foregroundColor;
        BackgroundColor = backgroundColor;
        Character = character;

        Tags = tags is null ? new HashSet<string>() : new HashSet<string>(tags);

        CollisionGroup = CollisionGroup.None;
        if (collisionGroups is not null)
        {
            foreach (string groupStr in collisionGroups)
            {
                CollisionGroup group = (CollisionGroup)Enum.Parse(typeof(CollisionGroup), groupStr, true);
                CollisionGroup |= group;
            }
        }
        
        Behaviors = new HashSet<TileBehavior>();
    }

    public Tile(Tile tile)
    {
        Id = tile.Id;
        ForegroundColor = tile.ForegroundColor;
        BackgroundColor = tile.BackgroundColor;
        Character = tile.Character;
        
        Tags = tile.Tags is null ? new HashSet<string>() : new HashSet<string>(tile.Tags);

        CollisionGroup = tile.CollisionGroup;

        Behaviors = new HashSet<TileBehavior>(tile.Behaviors);
    }
}