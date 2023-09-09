namespace SpelunkerUnearthed.Engine.Tiles;

public struct TileData
{
    public string ForegroundColor { get; private set; }
    public string BackgroundColor { get; private set; }
    public char Character { get; private set; }

    public TileData(string foregroundColor, string backgroundColor, char character)
    {
        ForegroundColor = foregroundColor;
        BackgroundColor = backgroundColor;
        Character = character;
    }
}