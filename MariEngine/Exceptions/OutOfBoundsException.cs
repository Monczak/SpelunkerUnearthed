using System;

namespace MariEngine.Exceptions;

public class OutOfBoundsException : Exception
{
    public OutOfBoundsException(Coord coord) : base($"{coord.ToString()} was out of bounds.")
    {
        
    }
}