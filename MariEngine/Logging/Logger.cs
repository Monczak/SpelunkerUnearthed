using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Pastel;

namespace MariEngine.Logging;

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
    
    public static void LogDebug(object message, [CallerMemberName] string caller = "", [CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
    {
        string codeInfo = $"{Path.GetFileName(path)}:{line} {caller}".Pastel(ConsoleColor.Gray);
        Log($"[{codeInfo}] {message}", LogSeverity.Debug);
    }

    private static void Log(object message, string severity)
    {
        Console.WriteLine($"[{DateTime.Now:hh:mm:ss.fff}] [{severity}] {message}");
    }
}