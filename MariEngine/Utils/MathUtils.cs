using System;

namespace MariEngine.Utils;

public static class MathUtils
{
    public static float Lerp(float a, float b, float t)
    {
        return (1f - t) * a + b * t;
    }

    public static float InverseLerp(float a, float b, float v)
    {
        return (v - a) / (b - a);
    }

    public static float Clamp(float x, float min, float max)
    {
        if (x >= max) return max;
        if (x <= min) return min;
        return x;
    }
}