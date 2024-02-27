using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MariEngine;
using MariEngine.Debugging;
using MariEngine.Logging;
using MariEngine.Persistence;
using MariEngine.Services;
using MariEngine.Utils;
using SpelunkerUnearthed.Scripts.MapGeneration.MapProcessors;
using YamlDotNet.Serialization;
using Random = MariEngine.Utils.Random;

namespace SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;

[SerializeCompressed]
public partial class CaveSystemLevel : ISaveable<CaveSystemLevel>
{
    public required int Depth { get; init; }
    public int MapGenSeed { get; init; }
    
    public List<Room> Rooms { get; private init; } = [];

    private Dictionary<Coord, Room> map = new();

    private readonly Queue<Room> roomQueue = new();
    private Random random;

    [YamlIgnore] public Room EntranceRoom => Rooms.Find(r => (r.Flags & RoomFlags.Entrance) != 0);
    
    public CoordBounds BoundingBox { get; private set; }
    
    public void Generate(RoomDecisionEngine decisionEngine, IEnumerable<IRoomLayoutProcessor> roomLayoutProcessors)
    {
        Rooms.Clear();
        map.Clear();
        roomQueue.Clear();

        random = ServiceRegistry.Get<RandomProvider>().Request(Constants.CaveSystemGenRng);
        
        // TODO: Get entrance room properties from a properties struct
        Room entranceRoom = new Room(new Coord(0, 0), new Coord(3, 3), 0, RoomFlags.Entrance);
        AddRoom(entranceRoom, decisionEngine);
        
        while (roomQueue.Count > 0)
        {
            GenerateRoom(decisionEngine);
        }
        
        foreach (var processor in roomLayoutProcessors)
            processor.ProcessRooms(Rooms);

        BoundingBox = CalculateBoundingBox();
    }

    public void GenerateRoom(RoomDecisionEngine decisionEngine)
    {
        if (roomQueue.Count == 0) return;
        Room room = roomQueue.Dequeue();
        
        HashSet<((Coord pos, AttachNode node), float weight)> placements = [];

        var newRoomSize = PickRoomSizeAndPlacements();

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
            foreach (AttachNode node in newRoom.AttachNodes)
            {
                if (map.ContainsKey(node.Position) 
                    && random.NextFloat() < decisionEngine.GetNeighborConnectionProbability(newRoom, map[node.Position]))
                {
                    newRoom.Connect(node, map[node.Position]);
                }
            }

            if (random.NextFloat() < decisionEngine.GetBranchingProbability(room))
                newRoomSize = PickRoomSizeAndPlacements();
            else
                break;
        }

        Coord PickRoomSizeAndPlacements()
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
        List<(Coord pos, AttachNode node)> positions = [];
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

    private CoordBounds CalculateBoundingBox()
    {
        Coord topLeft = Coord.Zero, bottomRight = Coord.Zero;

        foreach (Room room in Rooms)
        {
            if (room.Bounds.TopLeft.X < topLeft.X)
                topLeft.X = room.Bounds.TopLeft.X;
            if (room.Bounds.TopLeft.Y < topLeft.Y)
                topLeft.Y = room.Bounds.TopLeft.Y;
            
            if (room.Bounds.BottomRight.X > bottomRight.X)
                bottomRight.X = room.Bounds.BottomRight.X;
            if (room.Bounds.BottomRight.Y > bottomRight.Y)
                bottomRight.Y = room.Bounds.BottomRight.Y;
        }
        
        return CoordBounds.MakeCorners(topLeft, bottomRight);
    }

    private struct SerializationProxy
    {
        public int Depth { get; init; }
        public int MapGenSeed { get; init; }
        public List<Room> Rooms { get; init; }
        public CoordBounds BoundingBox { get; init; }
        public Dictionary<Room, HashSet<SubRoomConnection>> Connections { get; init; }
    }

    public void Serialize(Stream stream)
    {
        var proxy = new SerializationProxy
        {
            Depth = Depth,
            MapGenSeed = MapGenSeed,
            Rooms = Rooms,
            BoundingBox = BoundingBox,
            Connections = Rooms.Select(room => new { room, connections = room.Connections })
                .ToDictionary(data => data.room, data => data.connections)
        };
        
        var writer = new StreamWriter(stream);
        var serializedData = new SerializerBuilder()
            .EnsureRoundtrip()
            .WithTypeConverter(new Coord.YamlConverter())
            .WithTypeConverter(new PointOfInterest.YamlConverter())
            .Build()
            .Serialize(proxy);
        
        // Works around a YamlDotNet bug in serialization where there is no space between an anchor and a colon when the anchor is used as a mapping key
        writer.Write(YamlAnchorFixRegex().Replace(serializedData, m => $"*o{m.Groups[1].Value} :"));
        writer.Flush();
    }

    public static CaveSystemLevel Deserialize(Stream stream)
    {
        var reader = new StreamReader(stream);
        var data = new DeserializerBuilder()
            .WithTypeConverter(new Coord.YamlConverter())
            .WithTypeConverter(new PointOfInterest.YamlConverter())
            .Build()
            .Deserialize<SerializationProxy>(reader.ReadToEnd());

        var level = new CaveSystemLevel
        {
            Depth = data.Depth,
            MapGenSeed = data.MapGenSeed,
            Rooms = data.Rooms,
            BoundingBox = data.BoundingBox
        };
        
        foreach (var room in level.Rooms)
            room.Connections = data.Connections[room];

        return level;
    }

    [GeneratedRegex(@"\*o(\d+):")]
    private static partial Regex YamlAnchorFixRegex();
}