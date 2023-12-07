using System;

namespace MariEngine.Loading;

public class ResourceLoadingException(string message) : Exception(message);