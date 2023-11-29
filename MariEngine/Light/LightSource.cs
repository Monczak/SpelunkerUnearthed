using System;
using MariEngine.Tiles;
using Microsoft.Xna.Framework;
using MariEngine.Components;
using MariEngine.Utils;

namespace MariEngine.Light;

public abstract class LightSource : ICloneable
{
    private readonly Deferred<Color> color;
    private readonly Deferred<float> intensity;

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

    protected Tilemap Tilemap;

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

    protected LightSource(Color color, float intensity)
    {
        this.color = new Deferred<Color>(color);
        this.intensity = new Deferred<float>(intensity);
    }

    public void AttachTilemap(Tilemap tilemap)
    {
        Tilemap = tilemap;
    }

    protected abstract Color CalculateLight(Coord sourcePosition, Coord receiverPosition);
    protected abstract float CalculateAttenuation(Coord sourcePosition, Coord receiverPosition);

    public abstract CoordBounds? GetBounds(Coord sourcePosition);

    public Color GetLight(Coord sourcePosition, Coord receiverPosition)
    {
        return CalculateLight(sourcePosition, receiverPosition) * CalculateAttenuation(sourcePosition, receiverPosition) * Intensity;
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