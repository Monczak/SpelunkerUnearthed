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

    protected override Color CalculateLight(Coord position)
    {
        if ((position - OwnerEntity.Position).Magnitude > Radius)
            return default;
        return Color * CalculateAttenuation(position);
    }
    
    // TODO: Work out a way to optimize this, if necessary
    protected override float CalculateAttenuation(Coord position)
    {
        float distance = (OwnerEntity.Position - position).Magnitude;
        float normDistance = distance / Radius;
        
        float distanceAttenuation = 1 - MathF.Pow(normDistance, 3);

        float tileAttenuation = 1;
        
        // TODO: Improve the line generating algo (something different from Bresenham?)
        foreach (Coord coord in DrawingUtils.BresenhamLine(OwnerEntity.Position, position, endPreemptively: true))
        {
            Tile tile = OwnerEntity.Tilemap[coord];
            tileAttenuation *= 1 - tile.LightAttenuation;
        }

        return distanceAttenuation * tileAttenuation;
    }
}