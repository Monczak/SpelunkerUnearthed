using System.Collections.Generic;
using MariEngine;

namespace SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;

public readonly struct AttachNode
{
    public Coord Position { get; init; }
    public Direction Direction { get; init; }

    public AttachNode(Coord position, Direction direction)
    {
        Position = position;
        Direction = direction;
    }

    public IEnumerable<Coord> GetRoomPositions(Coord roomSize)
    {
        switch (Direction)
        {
            case Direction.Up:
                for (int x = Position.X - roomSize.X + 1; x <= Position.X; x++)
                    yield return new Coord(x, Position.Y - roomSize.Y + 1);
                break;
            case Direction.Right:
                for (int y = Position.Y - roomSize.Y + 1; y <= Position.Y; y++)
                    yield return new Coord(Position.X, y);
                break;
            case Direction.Down:
                for (int x = Position.X - roomSize.X + 1; x <= Position.X; x++)
                    yield return new Coord(x, Position.Y);
                break;
            case Direction.Left:
                for (int y = Position.Y - roomSize.Y + 1; y <= Position.Y; y++)
                    yield return new Coord(Position.X - roomSize.X + 1, y);
                break;
        }
    }
}