using System;

namespace SpelunkerUnearthed.Engine.Exceptions;

public class TileLoadingException : Exception
{
    public TileLoadingException(string message) : base(message)
    {
    }
}