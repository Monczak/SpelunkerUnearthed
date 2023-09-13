using System;

namespace SpelunkerUnearthed.Engine.Tiles;

public struct TileData
{
    public string ForegroundColor { get; private set; }
    public string BackgroundColor { get; private set; }
    public char Character { get; private set; }

    public string[] Tags { get; private set; }
    
    public string[] Behaviors { get; private set; }
    
    public string[] CollisionGroups { get; private set; }
}