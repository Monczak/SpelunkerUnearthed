using System;
using System.Collections.Generic;
using MariEngine;
using MariEngine.Debugging;
using MariEngine.Logging;
using MariEngine.Services;
using Random = MariEngine.Utils.Random;

namespace SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;

public class CaveSystemLevel
{
    public List<Room> Rooms { get; private set; } = new();

    private Dictionary<Coord, Room> map = new();

    private readonly Queue<Room> roomQueue = new();
    private Random random;
    
    public void Generate(RoomDecisionEngine decisionEngine)
    {
        Rooms.Clear();
        map.Clear();
        roomQueue.Clear();

        random = ServiceRegistry.Get<RandomProvider>().Request(Constants.CaveSystemGen);
        
        Room starterRoom = new Room(new Coord(0, 0), new Coord(3, 3), 0, RoomFlags.Entrance);
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
        
        HashSet<((Coord pos, AttachNode node), float weight)> placements = new();

        var newRoomSize = PickRoomSize();

        while (placements.Count > 0)
        {
            // Pick a random placement and remove it from the set
            var (pickedPosition, pickedNode) = random.PickWeighted(placements, out bool picked, remove: true);
            if (!picked)
                break;

            CoordBounds newRoomBounds = new(pickedPosition, newRoomSize);
            if (OverlapsRoom(newRoomBounds)) continue;

            Room newRoom = new(pickedPosition, newRoomSize, room.Distance + 1);
            AddRoom(newRoom, decisionEngine);

            room.Connect(pickedNode, newRoom);

            if (random.NextFloat() < decisionEngine.GetBranchingProbability(room))
                newRoomSize = PickRoomSize();
            else
                break;
        }
        
        // FIXME: Rooms generated last don't seem to connect to neighbors
        foreach (AttachNode node in room.AttachNodes)
        {
            if (map.ContainsKey(node.Position) 
                && random.NextFloat() < decisionEngine.GetNeighborConnectionProbability(room, map[node.Position]))
            {
                room.Connect(node, map[node.Position]);
            }
        }

        Coord PickRoomSize()
        {
            placements.Clear();
            
            // Pick a new room size
            Coord size = decisionEngine.PickRoomSize(room);

            // Get all possible placements for the new room
            var newRoomPlacements = GetNewRoomPlacements(room, size);

            // Rank potential room placements
            foreach (var placement in newRoomPlacements)
                placements.Add((placement, decisionEngine.GetPlacementWeight(room, placement, size, Rooms)));
            return size;
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

    private void AddRoom(Room newRoom, RoomDecisionEngine decisionEngine)
    {
        Rooms.Add(newRoom);
        
        if (random.NextFloat() < decisionEngine.GetContinueProbability(newRoom))
            roomQueue.Enqueue(newRoom);

        foreach (Coord coord in newRoom.RoomCoords)
            map[coord] = newRoom;
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