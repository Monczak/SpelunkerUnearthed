using System;

namespace MariEngine.Exceptions;

public class OutOfBoundsException(Coord coord) : Exception($"{coord.ToString()} was out of bounds.")
{
    public OutOfBoundsException(int x, int y) : this(new Coord(x, y))
    {
        
    }
}