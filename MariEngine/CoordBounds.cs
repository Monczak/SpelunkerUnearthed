using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace MariEngine;

public struct CoordBounds(Coord topLeft, Coord size)
{
    public Coord TopLeft { get; private init; } = topLeft;
    public Coord Size { get; private init; } = size;

    private Coord? topRight = null;
    private Coord? bottomLeft = null;
    private Coord? bottomRight = null;
    private Coord? center = null;
    private Vector2? exactCenter = null;

    public Coord TopRight
    {
        get
        {
            topRight ??= TopLeft + Coord.UnitX * (Size - Coord.One);
            return topRight.Value;
        }
    }
    public Coord BottomLeft
    {
        get 
        { 
            bottomLeft ??= TopLeft + Coord.UnitY * (Size - Coord.One);
            return bottomLeft.Value;
        }
    }

    public Coord BottomRight
    {
        get
        {
            bottomRight ??= TopLeft + Coord.One * (Size - Coord.One);
            return bottomRight.Value;
        }
    }

    public Coord Center
    {
        get
        {
            center ??= TopLeft + (Size - Coord.One) / 2;
            return center.Value;
        }
    }
    public Vector2 ExactCenter
    {
        get 
        { 
            exactCenter ??= (Vector2)TopLeft + (Vector2)Size / 2;
            return exactCenter.Value; 
        }
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
    
    public static CoordBounds MakeEnvelope(Coord coord1, Coord coord2) => MakeCorners(Coord.Min(coord1, coord2), Coord.Max(coord1, coord2));

    public static CoordBounds MakeEnvelope(ICollection<Coord> coords)
    {
        if (coords.Count < 2)
            throw new ArgumentException("Coord collection must be at least 2 elements long.");
        
        Coord minCoord = coords.First(), maxCoord = coords.First();
        foreach (Coord coord in coords)
        {
            minCoord = Coord.Min(coord, minCoord);
            maxCoord = Coord.Max(coord, maxCoord);
        }

        return MakeEnvelope(minCoord, maxCoord);
    }
    
    public bool PointInside(Coord point)
    {
        return point.X >= TopLeft.X && point.X <= TopLeft.X + Size.X - 1 && point.Y >= TopLeft.Y &&
               point.Y <= TopLeft.Y + Size.Y - 1;
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