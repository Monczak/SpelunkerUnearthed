using System;
using Microsoft.Xna.Framework;
using SpelunkerUnearthed.Engine.Components;
using SpelunkerUnearthed.Engine.Tiles;
using SpelunkerUnearthed.Engine.Utils;

namespace SpelunkerUnearthed.Engine.Light;

public abstract class LightSource : TileEntityComponent
{
    public Color Color { get; set; }

    protected abstract Color CalculateLight(Coord position);
    protected abstract float CalculateAttenuation(Coord position);

    public Color GetLight(Coord position)
    {
        return CalculateLight(position) * CalculateAttenuation(position);
    }
}