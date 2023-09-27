using System.Collections.Generic;
using MariEngine;

namespace SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;

public class Room
{
    public Coord Position { get; private set; }
    public Coord Size { get; private set; }
    
    public Dictionary<Coord, SubRoom> SubRooms { get; private set; }
    
    public List<SubRoomConnection> Connections { get; private set; }

    public IEnumerable<Coord> RoomCoords => SubRooms.Keys;
    public CoordBounds Bounds => new(Position, Size);

    public IEnumerable<AttachNode> AttachNodes
    {
        get
        {
            for (int x = Position.X; x < Position.X + Size.X; x++)
                yield return new AttachNode { Position = new Coord(x, Position.Y - 1), Direction = Direction.Up };
            
            for (int y = Position.Y; y < Position.Y + Size.Y; y++)
                yield return new AttachNode { Position = new Coord(Position.X + Size.X, y), Direction = Direction.Right };
            
            for (int x = Position.X + Size.X - 1; x >= Position.X; x--)
                yield return new AttachNode { Position = new Coord(x, Position.Y + Size.Y), Direction = Direction.Down };

            for (int y = Position.Y + Size.Y - 1; y >= Position.Y; y--)
                yield return new AttachNode { Position = new Coord(Position.X - 1, y), Direction = Direction.Left };
        }
    }

    public Room(Coord position, Coord size)
    {
        Position = position;
        Size = size;

        Connections = new List<SubRoomConnection>();
        SubRooms = new Dictionary<Coord, SubRoom>();
        for (int y = position.Y; y < position.Y + size.Y; y++)
        {
            for (int x = position.X; x < position.X + size.X; x++)
            {
                Coord subRoomPos = new(x, y);
                SubRooms.Add(subRoomPos, new SubRoom(this, subRoomPos));
            }
        }
    }
}