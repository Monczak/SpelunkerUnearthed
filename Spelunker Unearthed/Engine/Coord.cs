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

    public void Deconstruct(out int x, out int y)
    {
        x = X;
        y = Y;
    }
}