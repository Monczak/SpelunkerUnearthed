using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using SpelunkerUnearthed.Engine.Logging;
using SpelunkerUnearthed.Engine.Tiles;
using SpelunkerUnearthed.Engine.Utils;

namespace SpelunkerUnearthed.Engine.Light;

public class PointLight : LightSource
{
    public int Radius { get; set; }

    protected override Color CalculateLight(Coord sourcePosition, Coord receiverPosition)
    {
        return Color * CalculateAttenuation(sourcePosition, receiverPosition);
    }
    
    // TODO: Work out a way to optimize this, if necessary
    protected override float CalculateAttenuation(Coord sourcePosition, Coord receiverPosition)
    {
        float distance = (receiverPosition - sourcePosition).Magnitude;
        float normDistance = distance / Radius;
        
        float distanceAttenuation = 1 - MathF.Pow(normDistance, 3);

        float tileAttenuation = 1;
        
        // TODO: Improve the line generating algo (something different from Bresenham?)
        foreach (Coord coord in DrawingUtils.BresenhamLine(sourcePosition, receiverPosition, endPreemptively: true))
        {
            Tile tile = Tilemap[coord];
            tileAttenuation *= 1 - tile.LightAttenuation;
        }

        return distanceAttenuation * tileAttenuation;
    }

    public override bool IsInRange(Coord sourcePosition, Coord receiverPosition) =>
        (receiverPosition - sourcePosition).Magnitude <= Radius;

    protected override LightSource MakeClone() => new PointLight { Color = Color, Radius = Radius, Tilemap = Tilemap };
}