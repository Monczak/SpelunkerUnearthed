using System;

namespace MariEngine;

[Flags]
public enum Direction
{
    Up = 1 << 0,
    Right = 1 << 1,
    Down = 1 << 2,
    Left = 1 << 3,
    
    None = 0,
    All = ~None
}