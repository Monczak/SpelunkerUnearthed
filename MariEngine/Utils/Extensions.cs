using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace MariEngine.Utils;

public static class Extensions
{
    public static Vector2 Normalized(this Vector2 v) => v / v.Length();
    
    public static IEnumerable<T> ToEnumerable<T>(this T[,] target)
    {
        return target.Cast<T>();
    }

    public static Vector2 ToVector2(this Direction direction) => (Vector2)(Coord)direction;
    
    
    public static Direction GetDirection(this Vector2 v)
    {
        if (v == Vector2.Zero) return Direction.None;
        
        Vector2 absV = new(MathF.Abs(v.X), MathF.Abs(v.Y));
        if (absV.X > absV.Y)
        {
            return v.X > 0 ? Direction.Right : Direction.Left;
        }

        return v.Y > 0 ? Direction.Down : Direction.Up;
    }
}