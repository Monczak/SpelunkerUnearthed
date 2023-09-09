using System;

namespace SpelunkerUnearthed.Engine.Logging;

public static class Logger
{
    public static void Log(object message)
    {
        Log(message, LogSeverity.Info);
    }
    
    public static void LogWarning(object message)
    {
        Log(message, LogSeverity.Warning);
    }
    
    public static void LogError(object message)
    {
        Log(message, LogSeverity.Error);
    }
    
    public static void LogFatal(object message)
    {
        Log(message, LogSeverity.Fatal);
    }

    private static void Log(object message, string severity)
    {
        Console.WriteLine($"[{DateTime.Now:hh:mm:ss.fff}] [{severity}] {message}");
    }
}