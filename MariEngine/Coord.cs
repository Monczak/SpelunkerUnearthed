﻿using System;
using Microsoft.Xna.Framework;

namespace MariEngine;

public struct Coord
{
    public int X { get; set; }
    public int Y { get; set; }

    public Coord(int x, int y)
    {
        X = x;
        Y = y;
    }
    
    public Coord(Vector2 v)
    {
        X = (int)MathF.Floor(v.X);
        Y = (int)MathF.Floor(v.Y);
    }

    public Coord(float x, float y) : this(new Vector2(x, y))
    {
    }
    
    public override string ToString()
    {
        return $"({X}, {Y})";
    }

    public static explicit operator Vector2(Coord coord) => new(coord.X, coord.Y);
    public static explicit operator Coord(Vector2 v) => new(v);

    public static explicit operator Coord(Direction direction)
    {
        Coord result = Zero;
        if ((direction & Direction.Up) != 0)
            result += new Coord(0, -1);
        if ((direction & Direction.Right) != 0)
            result += new Coord(1, 0);
        if ((direction & Direction.Down) != 0)
            result += new Coord(0, 1);
        if ((direction & Direction.Left) != 0)
            result += new Coord(-1, 0);
        return result;
    }

    public static Coord operator +(Coord coord1, Coord coord2) => new(coord1.X + coord2.X, coord1.Y + coord2.Y);
    public static Coord operator -(Coord coord1, Coord coord2) => new(coord1.X - coord2.X, coord1.Y - coord2.Y);

    public static Coord operator *(Coord coord1, Coord coord2) => new(coord1.X * coord2.X, coord1.Y * coord2.Y);
    public static Coord operator *(Coord coord1, int scale) => new(coord1.X * scale, coord1.Y * scale);

    public static Coord operator /(Coord coord1, Coord coord2) => new(coord1.X / coord2.X, coord1.Y / coord2.Y);
    public static Coord operator /(Coord coord1, int divisor) => new(coord1.X / divisor, coord1.Y / divisor);

    public static Vector2 operator *(Vector2 v, Coord c) => new(v.X * c.X, v.Y * c.Y);
    public static Vector2 operator *(Coord c, Vector2 v) => new(v.X * c.X, v.Y * c.Y);

    public void Deconstruct(out int x, out int y)
    {
        x = X;
        y = Y;
    }

    public override bool Equals(object obj)
    {
        if (obj is null || GetType() != obj.GetType())
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
    public static Coord One => new(1, 1);
    public static Coord UnitX => new(1, 0);
    public static Coord UnitY => new(0, 1);

    public int SqrMagnitude => X * X + Y * Y;
    public float Magnitude => (float)Math.Sqrt(SqrMagnitude);

    public static Coord Abs(Coord coord) => new(Math.Abs(coord.X), Math.Abs(coord.Y));
    public static Coord Orthogonal(Coord coord) => new(-coord.Y, coord.X);

    public static int Dot(Coord coord1, Coord coord2) => coord1.X * coord2.X + coord1.Y * coord2.Y;

    public Direction Direction
    {
        get
        {
            if (this == Zero) return Direction.None;
        
            Vector2 absC = new(MathF.Abs(X), MathF.Abs(Y));
            if (absC.X > absC.Y)
            {
                return X > 0 ? Direction.Right : Direction.Left;
            }

            return Y > 0 ? Direction.Down : Direction.Up;
        }
    }
}