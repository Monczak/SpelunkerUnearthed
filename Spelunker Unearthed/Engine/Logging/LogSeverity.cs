using System;
using Pastel;

namespace SpelunkerUnearthed.Engine.Logging;

public static class LogSeverity
{
    public static readonly string Info      = "INFO".Pastel(ConsoleColor.Green);
    public static readonly string Warning   = "WARN".Pastel(ConsoleColor.Yellow);
    public static readonly string Error     = "ERROR".Pastel(ConsoleColor.Red);
    public static readonly string Fatal     = "FATAL".Pastel(ConsoleColor.DarkRed);
}