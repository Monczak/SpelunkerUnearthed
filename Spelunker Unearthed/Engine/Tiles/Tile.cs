using Microsoft.Xna.Framework;
using SpelunkerUnearthed.Engine.Utils;

namespace SpelunkerUnearthed.Engine.Tiles;

public class Tile
{
    public Color ForegroundColor { get; }
    public Color BackgroundColor { get; }
    public char Character { get; }
    
    public Tile(TileData data)
    {
        ForegroundColor = ColorUtils.FromHex(data.ForegroundColor);
        BackgroundColor = ColorUtils.FromHex(data.BackgroundColor);
        Character = data.Character;
    }

    public Tile(Color foregroundColor, Color backgroundColor, char character)
    {
        ForegroundColor = foregroundColor;
        BackgroundColor = backgroundColor;
        Character = character;
    }
}