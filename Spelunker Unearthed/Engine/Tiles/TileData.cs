using System;

namespace SpelunkerUnearthed.Engine.Tiles;

public struct TileData
{
    public string ForegroundColor { get; init; }
    public string BackgroundColor { get; init; }
    public char Character { get; init; }

    public string[] Tags { get; init; }
    
    public string[] Behaviors { get; init; }
    
    public TileLightData? Light { get; init; }
    public float LightAttenuation { get; init; }
    
    public string[] CollisionGroups { get; init; }
}