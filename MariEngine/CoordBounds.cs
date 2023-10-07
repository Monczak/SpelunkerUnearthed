using System;
using System.Collections.Generic;

namespace MariEngine;

public struct CoordBounds
{
    public Coord TopLeft { get; set; }
    public Coord Size { get; set; }

    public Coord TopRight => TopLeft + Coord.UnitX * (Size - Coord.One);
    public Coord BottomLeft => TopLeft + Coord.UnitY * (Size - Coord.One);
    public Coord BottomRight => TopLeft + Coord.One * (Size - Coord.One);

    public Coord Center => TopLeft + (Size - Coord.One) / 2;

    public CoordBounds(Coord topLeft, Coord size)
    {
        TopLeft = topLeft;
        Size = size;
    }

    public static CoordBounds MakeCorners(Coord topLeft, Coord bottomRight) 
    {
        CoordBounds bounds = new()
        {
            TopLeft = topLeft,
            Size = bottomRight - topLeft + Coord.One
        };
        return bounds;
    }

    public bool PointInside(Coord point)
    {
        return point.X >= TopLeft.X && point.X <= TopRight.X && point.Y >= TopLeft.Y && point.Y <= BottomLeft.Y;
    }

    public bool Overlaps(CoordBounds bounds)
    {
        return GetOverlap(this, bounds) is not null;
    }
    
    public static CoordBounds? GetOverlap(CoordBounds bounds1, CoordBounds bounds2)
    {
        Coord topLeft = new((int)MathF.Max(bounds1.TopLeft.X, bounds2.TopLeft.X), (int)MathF.Max(bounds1.TopLeft.Y, bounds2.TopLeft.Y));
        Coord bottomRight = new((int)MathF.Min(bounds1.BottomRight.X, bounds2.BottomRight.X),
            (int)MathF.Min(bounds1.BottomRight.Y, bounds2.BottomRight.Y));

        if (topLeft.X <= bottomRight.X && topLeft.Y <= bottomRight.Y)
            return MakeCorners(topLeft, bottomRight);
        return null;
    }

    public IEnumerable<Coord> Coords
    {
        get
        {
            for (int y = TopLeft.Y; y <= BottomRight.Y; y++)
            {
                for (int x = TopLeft.X; x <= BottomRight.X; x++)
                {
                    yield return new Coord(x, y);
                }
            }
        }
    }
}