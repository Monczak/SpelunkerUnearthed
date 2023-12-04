using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Pastel;

namespace MariEngine.Logging;

public static class Logger
{
    public class LoggerStopwatch
    {
        private Stopwatch stopwatch = new();
        public TimeSpan? LastTime { get; private set;}

        public TimeSpan Elapsed => stopwatch.Elapsed;

        public LoggerStopwatch()
        {
            stopwatch.Start();
        }

        public void UpdateLastTime() => LastTime = Elapsed;
        public void Stop() => stopwatch.Stop();
    }
    
    public static void Log(object message, LoggerStopwatch stopwatch = null)
    {
        Log(message, LogSeverity.Info, stopwatch);
    }
    
    public static void LogWarning(object message, LoggerStopwatch stopwatch = null)
    {
        Log(message, LogSeverity.Warning, stopwatch);
    }
    
    public static void LogError(object message, LoggerStopwatch stopwatch = null)
    {
        Log(message, LogSeverity.Error, stopwatch);
    }
    
    public static void LogFatal(object message, LoggerStopwatch stopwatch = null)
    {
        Log(message, LogSeverity.Fatal, stopwatch);
    }
    
    public static void LogDebug(object message, LoggerStopwatch stopwatch = null, [CallerMemberName] string caller = "", [CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
    {
        string codeInfo = $"{Path.GetFileName(path)}:{line} {caller}".Pastel(ConsoleColor.Gray);
        Log($"[{codeInfo}] {message}", LogSeverity.Debug, stopwatch);
    }

    public static LoggerStopwatch StartStopwatch()
    {
        return new LoggerStopwatch();
    }

    private static void Log(object message, string severity, LoggerStopwatch stopwatch = null)
    {
        string stopwatchInfo = stopwatch is not null
            ? $"{stopwatch.Elapsed.TotalSeconds:F3}{(stopwatch.LastTime is not null ? $" (+{stopwatch.Elapsed.TotalSeconds - stopwatch.LastTime.Value.TotalSeconds:F3})" : "")}"
            : null;
        
        Console.WriteLine($"[{DateTime.Now:hh:mm:ss.fff}] " +
                          $"[{severity}] " +
                          (stopwatchInfo is not null 
                              ? $"[{stopwatchInfo.Pastel(ConsoleColor.Gray)}] " 
                              : ""
                          ) +
                          $"{message}");
        stopwatch?.UpdateLastTime();
    }
}