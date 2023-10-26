using System;

namespace MariEngine.Exceptions;

public class ContentLoadingException : Exception
{
    public ContentLoadingException(string message) : base(message)
    {
    }
}