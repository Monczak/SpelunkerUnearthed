using System;
using System.Diagnostics.CodeAnalysis;
using MariEngine.Tiles;
using MariEngine.Utils;
using Microsoft.Xna.Framework;
using MariEngine.Logging;

namespace MariEngine.Light;

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
        
        // TODO: There is a lot of unnecessary calculation going on here, since we could be drawing lines over something we've calculated already with another line
        // Precomputing attenuation with lines towards edges of bounds will make this faster
        foreach (Coord coord in DrawingUtils.BresenhamLine(sourcePosition, receiverPosition, endPreemptively: true))
        {
            Tile tile = Tilemap[coord];
            tileAttenuation *= 1 - tile.LightAttenuation;
        }

        return distanceAttenuation * tileAttenuation;
    }

    public override Bounds? GetBounds(Coord sourcePosition)
    {
        return new Bounds((Vector2)(sourcePosition - Coord.One * Radius), Vector2.One * (Radius * 2 + 1));
    }

    protected override LightSource MakeClone() => new PointLight { Color = Color, Radius = Radius, Tilemap = Tilemap };
}