using System;
using MariEngine.Tiles;
using Microsoft.Xna.Framework;
using MariEngine.Components;
using MariEngine.Utils;

namespace MariEngine.Light;

public abstract class LightSource(Color color, float intensity) : ICloneable
{
    private readonly Deferred<Color> color = new(color);
    private readonly Deferred<float> intensity = new(intensity);

    public Color Color
    {
        get => color.Get();
        set
        {
            color.Set(value);
            Dirty = true;
        }
    }

    public float Intensity
    {
        get => intensity.Get();
        set
        {
            intensity.Set(value);
            Dirty = true;
        }
    }

    private bool dirty;
    internal bool Dirty
    {
        get => dirty;
        set
        {
            dirty = value;
            if (dirty) OnDirty?.Invoke(this);
        }
    }

    public event Action<LightSource> OnDirty;

    protected abstract Color CalculateLight(Tilemap tilemap, Coord sourcePosition, Coord receiverPosition);
    protected abstract float CalculateAttenuation(Tilemap tilemap, Coord sourcePosition, Coord receiverPosition);

    public abstract CoordBounds? GetBounds(Coord sourcePosition);

    public Color GetLight(Tilemap tilemap, Coord sourcePosition, Coord receiverPosition)
    {
        return CalculateLight(tilemap, sourcePosition, receiverPosition) * CalculateAttenuation(tilemap, sourcePosition, receiverPosition) * Intensity;
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