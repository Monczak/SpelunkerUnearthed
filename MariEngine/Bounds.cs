using System;
using Microsoft.Xna.Framework;

namespace MariEngine;

public struct Bounds
{
    public Vector2 TopLeft { get; set; }
    public Vector2 Size { get; set; }

    public Vector2 TopRight => TopLeft + Vector2.UnitX * Size;
    public Vector2 BottomLeft => TopLeft + Vector2.UnitY * Size;
    public Vector2 BottomRight => TopLeft + Vector2.One * Size;

    public Bounds(Vector2 topLeft, Vector2 size)
    {
        TopLeft = topLeft;
        Size = size;
    }

    public static Bounds MakeCorners(Vector2 topLeft, Vector2 bottomRight)
    {
        Bounds bounds = new()
        {
            TopLeft = topLeft,
            Size = bottomRight - topLeft
        };
        return bounds;
    }
    
    public bool PointInside(Vector2 point)
    {
        return point.X >= TopLeft.X && point.X <= TopRight.X && point.Y >= TopLeft.Y && point.Y <= BottomLeft.Y;
    }

    public static Bounds? Overlap(Bounds bounds1, Bounds bounds2)
    {
        Vector2 topLeft = new(MathF.Max(bounds1.TopLeft.X, bounds2.TopLeft.X), MathF.Max(bounds1.TopLeft.Y, bounds2.TopLeft.Y));
        Vector2 bottomRight = new(MathF.Min(bounds1.BottomRight.X, bounds2.BottomRight.X),
            MathF.Min(bounds1.BottomRight.Y, bounds2.BottomRight.Y));

        if (topLeft.X < bottomRight.X && topLeft.Y < bottomRight.Y)
            return MakeCorners(topLeft, bottomRight);
        return null;
    }

    public static explicit operator Bounds(CoordBounds bounds) => new((Vector2)bounds.TopLeft, (Vector2)bounds.Size);
}