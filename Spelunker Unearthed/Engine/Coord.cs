using System;
using Microsoft.Xna.Framework;

namespace SpelunkerUnearthed.Engine;

public struct Coord
{
    public int X { get; set; }
    public int Y { get; set; }

    public Coord(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override string ToString()
    {
        return $"({X}, {Y})";
    }

    public static explicit operator Vector2(Coord coord) => new(coord.X, coord.Y);

    public static Coord operator +(Coord coord1, Coord coord2) => new(coord1.X + coord2.X, coord1.Y + coord2.Y);
    public static Coord operator -(Coord coord1, Coord coord2) => new(coord1.X - coord2.X, coord1.Y - coord2.Y);

    public static Coord operator *(Coord coord1, Coord coord2) => new(coord1.X * coord2.X, coord1.Y * coord2.Y);

    public static Vector2 operator *(Vector2 v, Coord c) => new(v.X * c.X, v.Y * c.Y);
    public static Vector2 operator *(Coord c, Vector2 v) => new(v.X * c.X, v.Y * c.Y);

    public void Deconstruct(out int x, out int y)
    {
        x = X;
        y = Y;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        Coord other = (Coord)obj;
        return Equals(other);
    }

    private bool Equals(Coord other) => X == other.X && Y == other.Y;

    public override int GetHashCode()
    {
        return X.GetHashCode() ^ Y.GetHashCode();
    }

    public static bool operator ==(Coord a, Coord b) => a.Equals(b);
    public static bool operator !=(Coord a, Coord b) => !(a == b);

    public static Coord Zero => new(0, 0);

    public int SqrMagnitude => X * X + Y * Y;
    public float Magnitude => (float)Math.Sqrt(SqrMagnitude);

    public static Coord Abs(Coord coord) => new(Math.Abs(coord.X), Math.Abs(coord.Y));
    public static Coord Orthogonal(Coord coord) => new(-coord.Y, coord.X);
}