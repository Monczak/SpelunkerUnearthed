using System;
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

    private readonly Queue<Room> roomQueue = new();

    // FIXME: This takes longer to finish every time it's run?
    public void Generate(RoomDecisionEngine decisionEngine)
    {
        Rooms.Clear();
        map.Clear();
        roomQueue.Clear();
        
        Room starterRoom = new Room(new Coord(0, 0), new Coord(3, 3), 0);
        AddRoom(starterRoom, decisionEngine);
        
        while (roomQueue.Count > 0)
        {
            GenerateRoom(decisionEngine);
        }
    }

    public void GenerateRoom(RoomDecisionEngine decisionEngine)
    {
        if (roomQueue.Count == 0) return;
        Room room = roomQueue.Dequeue();
        
        // Pick a new room size
        Coord newRoomSize = decisionEngine.PickRoomSize(room);

        // Get all possible placements for the new room
        var newRoomPlacements = GetNewRoomPlacements(room, newRoomSize);
        
        // Rank potential room placements
        HashSet<((Coord pos, AttachNode node), float weight)> placements = new();
        foreach (var placement in newRoomPlacements)
            placements.Add((placement, decisionEngine.GetPlacementWeight(room, placement, newRoomSize, Rooms)));
        
        while (placements.Count > 0)
        {
            // Pick a random placement and remove it from the set
            var (position, node) = ServiceRegistry.Get<RandomNumberGenerator>().PickWeighted(placements, out bool picked, remove: true);
            if (!picked)
            {
                break;
            }
            
            CoordBounds newRoomBounds = new(position, newRoomSize);
            if (OverlapsRoom(newRoomBounds)) continue;

            Room newRoom = new(position, newRoomSize, room.Distance + 1);
            AddRoom(newRoom, decisionEngine);

            Coord subRoomPos = node.Position - (Coord)node.Direction;
            SubRoom subRoom = room.SubRooms[subRoomPos];
            SubRoom newSubRoom = newRoom.SubRooms[node.Position];

            SubRoomConnection connection = new(subRoom, newSubRoom);
            room.Connections.Add(connection);
            newRoom.Connections.Add(connection.Reversed);

            if (ServiceRegistry.Get<RandomNumberGenerator>().NextFloat() > decisionEngine.GetBranchingProbability(room))
                break;
        }
    }

    private List<(Coord pos, AttachNode node)> GetNewRoomPlacements(Room room, Coord newRoomSize)
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

    private void AddRoom(Room room, RoomDecisionEngine decisionEngine)
    {
        Rooms.Add(room);
        
        if (ServiceRegistry.Get<RandomNumberGenerator>().NextFloat() < decisionEngine.GetContinueProbability(room))
            roomQueue.Enqueue(room);

        foreach (Coord coord in room.RoomCoords)
            map[coord] = room;
    }

    private bool OverlapsRoom(Room room) => OverlapsRoom(room.Bounds);

    private bool OverlapsRoom(CoordBounds bounds)
    {
        foreach (Room room in Rooms)
        {
            if (room.Bounds.Overlaps(bounds)) 
                return true;
        }
        return false;
    }
}