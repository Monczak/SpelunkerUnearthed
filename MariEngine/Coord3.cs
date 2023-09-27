using System;
using Microsoft.Xna.Framework;

namespace MariEngine;

public struct Coord3
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }

    public Coord3(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public override string ToString()
    {
        return $"({X}, {Y}, {Z})";
    }

    public static explicit operator Vector3(Coord3 coord) => new(coord.X, coord.Y, coord.Z);
    public static explicit operator Coord3(Vector3 v) => new((int)MathF.Floor(v.X), (int)MathF.Floor(v.Y), (int)MathF.Floor(v.Z));

    public static Coord3 operator +(Coord3 coord1, Coord3 coord2) => new(coord1.X + coord2.X, coord1.Y + coord2.Y, coord1.Z + coord2.Z);
    public static Coord3 operator -(Coord3 coord1, Coord3 coord2) => new(coord1.X - coord2.X, coord1.Y - coord2.Y, coord1.Z - coord2.Z);

    public static Coord3 operator *(Coord3 coord1, Coord3 coord2) => new(coord1.X * coord2.X, coord1.Y * coord2.Y, coord1.Z * coord2.Z);
    public static Coord3 operator *(Coord3 coord1, int scale) => new(coord1.X * scale, coord1.Y * scale, coord1.Z * scale);

    public static Vector3 operator *(Vector3 v, Coord3 c) => new(v.X * c.X, v.Y * c.Y, v.Z * c.Z);
    public static Vector3 operator *(Coord3 c, Vector3 v) => new(v.X * c.X, v.Y * c.Y, v.Z * c.Z);

    public void Deconstruct(out int x, out int y, out int z)
    {
        x = X;
        y = Y;
        z = Z;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        Coord3 other = (Coord3)obj;
        return Equals(other);
    }

    private bool Equals(Coord3 other) => X == other.X && Y == other.Y && Z == other.Z;

    public override int GetHashCode()
    {
        return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
    }

    public static bool operator ==(Coord3 a, Coord3 b) => a.Equals(b);
    public static bool operator !=(Coord3 a, Coord3 b) => !(a == b);

    public static Coord3 Zero => new(0, 0, 0);
    public static Coord3 One => new(1, 1, 1);

    public int SqrMagnitude => X * X + Y * Y + Z * Z;
    public float Magnitude => (float)Math.Sqrt(SqrMagnitude);

    public static Coord3 Abs(Coord3 coord) => new(Math.Abs(coord.X), Math.Abs(coord.Y), Math.Abs(coord.Z));
}