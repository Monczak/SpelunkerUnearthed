﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace MariEngine.Utils;

public static class MathUtils
{
    public const float Pi = 3.14159f;

    public const float Rad2Deg = 180 / Pi;
    public const float Deg2Rad = Pi / 180;
    
    public static float LerpUnclamped(float a, float b, float t)
    {
        return (1 - t) * a + b * t;
    }

    public static float Lerp(float a, float b, float t)
    {
        return Clamp(LerpUnclamped(a, b, t), a, b);
    }

    public static float InverseLerp(float a, float b, float v)
    {
        if (Math.Abs(a - b) < float.Epsilon) return 1;
        return (v - a) / (b - a);
    }

    public static float Remap(float i1, float i2, float o1, float o2, float v)
    {
        return Lerp(o1, o2, InverseLerp(i1, i2, v));
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

    public static float DiamondAngle(Vector2 v)
    {
        var (x, y) = v;
        if (y >= 0)
            return x >= 0 ? y / (x + y) : 1 - x / ( -x + y);
        return x < 0 ? 2 - y / (-x - y) : 3 + x / (x - y);
    }

    public static float GeometricMean(this IEnumerable<float> collection)
    {
        var count = 0;
        var product = 1f;
        foreach (var x in collection)
        {
            count++;
            product *= x;
        }

        return MathF.Pow(product, 1f / count);
    }

    public static float HarmonicMean(this IEnumerable<float> collection)
    {
        var count = 0;
        var reciprocalSum = 0f;
        foreach (var x in collection)
        {
            count++;
            reciprocalSum += 1f / x;
        }
        return count / reciprocalSum;
    }

    public static IEnumerable<float> Normalize(this IEnumerable<float> collection)
    {
        var copy = new List<float>(collection);
        var min = copy.Min();
        var max = copy.Max();
        foreach (var x in copy)
            yield return InverseLerp(min, max, x);
    }
}