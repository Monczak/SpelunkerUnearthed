using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SpelunkerUnearthed.Engine.Utils;

namespace SpelunkerUnearthed.Engine.Tiles;

public class Tile
{
    public Color ForegroundColor { get; }
    public Color BackgroundColor { get; }
    public char Character { get; }
    
    public Tilemap OwnerTilemap { get; set; }
    public HashSet<TileBehavior> Behaviors { get; }
    
    public Tile(TileData data) : this(
        ColorUtils.FromHex(data.ForegroundColor), 
        ColorUtils.FromHex(data.BackgroundColor), 
        data.Character)
    {
        
    }

    public Tile(Color foregroundColor, Color backgroundColor, char character)
    {
        ForegroundColor = foregroundColor;
        BackgroundColor = backgroundColor;
        Character = character;

        Behaviors = new HashSet<TileBehavior>();
    }

    public Tile(Tile tile)
    {
        ForegroundColor = tile.ForegroundColor;
        BackgroundColor = tile.BackgroundColor;
        Character = tile.Character;

        Behaviors = new HashSet<TileBehavior>(tile.Behaviors);
    }
}