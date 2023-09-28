using System.Collections.Generic;
using MariEngine;
using MariEngine.Debugging;
using MariEngine.Logging;
using MariEngine.Services;

namespace SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;

public class CaveSystemLevel
{
    public List<Room> Rooms { get; private set; } = new();

    private Dictionary<Coord, Room> map = new();

    private Queue<Room> roomQueue = new();

    private int distanceLimit = 7;     // TODO: Get this from a properties struct
    private int connectionLimit = 2;    // TODO: Get this from a properties struct

    public void Generate()
    {
        roomQueue.Clear();
        
        Room starterRoom = new Room(new Coord(0, 0), new Coord(3, 3), 0);
        AddRoom(starterRoom);
        
        /*while (roomQueue.Count > 0)
        {
            GenerateRoom();
        }*/
    }

    public void GenerateRoom()
    {
        if (roomQueue.Count == 0) return;
        Room room = roomQueue.Dequeue();

        // TODO: Sample room size from a distribution from a properties struct
        Coord newRoomSize = new(ServiceRegistry.Get<RandomNumberGenerator>().Next(1, 4), ServiceRegistry.Get<RandomNumberGenerator>().Next(1, 4));

        var newRoomPositions = GetNewRoomPositions(room, newRoomSize);
        ServiceRegistry.Get<RandomNumberGenerator>().Shuffle(newRoomPositions);
        foreach ((Coord position, AttachNode node) in newRoomPositions)
        {
            CoordBounds newRoomBounds = new(position, newRoomSize);
            if (OverlapsRoom(newRoomBounds)) continue;

            Room newRoom = new(position, newRoomSize, room.Distance + 1);
            AddRoom(newRoom);

            Coord subRoomPos = node.Position - (Coord)node.Direction;
            SubRoom subRoom = room.SubRooms[subRoomPos];
            SubRoom newSubRoom = newRoom.SubRooms[node.Position];

            SubRoomConnection connection = new(subRoom, newSubRoom);
            room.Connections.Add(connection);
            newRoom.Connections.Add(connection.Reversed);
            
            if (room.Connections.Count >= connectionLimit)
                break;
        }
    }

    private List<(Coord pos, AttachNode node)> GetNewRoomPositions(Room room, Coord newRoomSize)
    {
        List<(Coord pos, AttachNode node)> positions = new();
        foreach (AttachNode node in room.AttachNodes)
        {
            foreach (Coord position in node.GetRoomPositions(newRoomSize))
            {
                positions.Add((position, node));
            }
        }

        return positions;
    }

    private void AddRoom(Room room)
    {
        Rooms.Add(room);
        
        if (room.Distance <= distanceLimit)
            roomQueue.Enqueue(room);

        foreach (Coord coord in room.RoomCoords)
            map[coord] = room;
    }

    private bool OverlapsRoom(Room room) => OverlapsRoom(room.Bounds);

    private bool OverlapsRoom(CoordBounds bounds)
    {
        foreach (Coord coord in bounds.Coords)
        {
            if (map.ContainsKey(coord))
                return true;
        }

        return false;
    }
}