using System;
using Microsoft.Xna.Framework;

namespace MariEngine.Utils;

public static class MathUtils
{
    public static float Lerp(float a, float b, float t)
    {
        return (1 - t) * a + b * t;
    }

    public static float InverseLerp(float a, float b, float v)
    {
        if (Math.Abs(a - b) < float.Epsilon) return 1;
        return (v - a) / (b - a);
    }

    public static float Clamp(float x, float min, float max)
    {
        if (x >= max) return max;
        if (x <= min) return min;
        return x;
    }

    public static float Cross(Vector2 a, Vector2 b)
    {
        return a.X * b.Y - a.Y * b.X;
    }

    public static float AngleCosine(Vector2 a, Vector2 b)
    {
        return Vector2.Dot(a, b) / (a.Length() * b.Length());
    }

    public static float AngleBetween(Vector2 a, Vector2 b)
    {
        return MathF.Acos(AngleCosine(a, b));
    }

    public static float SmallestAngleBetween(Vector2 a, Vector2 b)
    {
        float angle = AngleBetween(a, b);
        return Cross(a, b) < 0 ? MathHelper.TwoPi - angle : angle;
    }
}