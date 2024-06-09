using System;

namespace MariEngine.Loading;

public class ComponentLoadingException(string message) : Exception(message);