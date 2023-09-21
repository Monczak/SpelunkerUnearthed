using System;

namespace MariEngine.Exceptions;

public class TileLoadingException : Exception
{
    public TileLoadingException(string message) : base(message)
    {
    }
}