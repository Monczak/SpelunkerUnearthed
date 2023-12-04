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
    All = ~None,
    
    Horizontal = Left | Right,
    Vertical = Up | Down,
}

public static class DirectionExtensions
{
    public static Direction Reversed(this Direction direction) => direction switch
    {
        Direction.Up => Direction.Down,
        Direction.Down => Direction.Up,
        Direction.Left => Direction.Right,
        Direction.Right => Direction.Left,
        Direction.None => Direction.None,
        Direction.All => Direction.All,
        Direction.Horizontal => Direction.Vertical,
        Direction.Vertical => Direction.Horizontal,
        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
    };
}