using System;
using MariEngine.Tiles;
using Microsoft.Xna.Framework;
using MariEngine.Components;
using MariEngine.Utils;

namespace MariEngine.Light;

public abstract class LightSource : ICloneable
{
    private readonly Deferred<Color> color;

    public Color Color
    {
        get => color.Get();
        set
        {
            color.Set(value);
            Dirty = true;
        }
    }

    protected Tilemap Tilemap;
    
    internal bool Dirty { get; set; }

    protected LightSource(Color color)
    {
        this.color = new Deferred<Color>(color);
    }

    public void AttachTilemap(Tilemap tilemap)
    {
        Tilemap = tilemap;
    }

    protected abstract Color CalculateLight(Coord sourcePosition, Coord receiverPosition);
    protected abstract float CalculateAttenuation(Coord sourcePosition, Coord receiverPosition);

    public abstract Bounds? GetBounds(Coord sourcePosition);

    public Color GetLight(Coord sourcePosition, Coord receiverPosition)
    {
        return CalculateLight(sourcePosition, receiverPosition) * CalculateAttenuation(sourcePosition, receiverPosition);
    }

    public object Clone()
    {
        return MakeClone();
    }

    protected abstract LightSource MakeClone();

    public void UpdateAllProperties()
    {
        color.Update();
        UpdateProperties();
    }
    protected abstract void UpdateProperties();
}