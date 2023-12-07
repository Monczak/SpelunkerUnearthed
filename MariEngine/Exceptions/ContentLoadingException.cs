using System;

namespace MariEngine.Exceptions;

public class ContentLoadingException(string message) : Exception(message);