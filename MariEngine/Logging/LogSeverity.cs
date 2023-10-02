using System;
using Pastel;

namespace MariEngine.Logging;

public static class LogSeverity
{
    public static readonly string Info      = "INFO".Pastel(ConsoleColor.Green);
    public static readonly string Warning   = "WARN".Pastel(ConsoleColor.Yellow);
    public static readonly string Error     = "ERROR".Pastel(ConsoleColor.Red);
    public static readonly string Fatal     = "FATAL".Pastel(ConsoleColor.DarkRed);
    public static readonly string Debug     = "DEBUG".Pastel(ConsoleColor.DarkGreen);
}