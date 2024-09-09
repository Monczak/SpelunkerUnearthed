using System;
using System.Diagnostics.CodeAnalysis;
using MariEngine.Tiles;
using MariEngine.Utils;
using Microsoft.Xna.Framework;
using MariEngine.Logging;

namespace MariEngine.Light;

public class PointLight(Color color, float intensity, int radius) : LightSource(color, intensity)
{
    private readonly Deferred<int> radius = new(radius);

    public int Radius
    {
        get => radius.Get();
        set
        {
            radius.Set(value);
            Dirty = true;
        }
    }

    protected override Color CalculateLight(Tilemap tilemap, Coord sourcePosition, Coord receiverPosition)
    {
        return Color;
    }
    
    // TODO: Work out a way to optimize this, if necessary
    protected override float CalculateAttenuation(Tilemap tilemap, Coord sourcePosition, Coord receiverPosition)
    {
        float distance = (receiverPosition - sourcePosition).Magnitude;
        float normDistance = distance / radius.Get();
        
        float distanceAttenuation = 1 - MathF.Pow(normDistance, 3);

        float tileAttenuation = 1;
        
        // TODO: There is a lot of unnecessary calculation going on here, since we could be drawing lines over something we've calculated already with another line
        // Precomputing attenuation with lines towards edges of bounds will make this faster
        foreach (Coord coord in DrawingUtils.BresenhamLine(sourcePosition, receiverPosition, endPreemptively: true))
        {
            if (!tilemap.IsInBounds(coord)) continue;
            
            Tile tile = tilemap.Get(coord, TilemapLayer.Base);
            if (tile is null) continue;
            
            tileAttenuation *= 1 - tile.LightAttenuation;
        }

        return distanceAttenuation * tileAttenuation;
    }

    public override CoordBounds? GetBounds(Coord sourcePosition)
    {
        return new CoordBounds(sourcePosition - Coord.One * radius.Get(), Coord.One * (radius.Get() * 2 + 1));
    }

    protected override LightSource MakeClone() => new PointLight(Color, Intensity, Radius);

    protected override void UpdateProperties()
    {
        radius.Update();
    }
}