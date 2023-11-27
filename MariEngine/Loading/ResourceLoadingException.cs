using System;

namespace MariEngine.Loading;

public class ResourceLoadingException : Exception
{
    public ResourceLoadingException(string message) : base(message)
    {
        
    }
}