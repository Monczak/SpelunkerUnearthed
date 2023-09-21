using System;
using MariEngine.Tiles;
using Microsoft.Xna.Framework;
using MariEngine.Components;
using MariEngine.Utils;

namespace MariEngine.Light;

public abstract class LightSource : ICloneable
{
    public Color Color { get; set; }
    protected Tilemap Tilemap;

    public void AttachTilemap(Tilemap tilemap)
    {
        Tilemap = tilemap;
    }

    protected abstract Color CalculateLight(Coord sourcePosition, Coord receiverPosition);
    protected abstract float CalculateAttenuation(Coord sourcePosition, Coord receiverPosition);
    
    public abstract bool IsInRange(Coord sourcePosition, Coord receiverPosition);

    public Color GetLight(Coord sourcePosition, Coord receiverPosition)
    {
        return CalculateLight(sourcePosition, receiverPosition) * CalculateAttenuation(sourcePosition, receiverPosition);
    }

    public object Clone()
    {
        return MakeClone();
    }

    protected abstract LightSource MakeClone();
}