using System;
using System.Collections.Generic;
using MariEngine;
using MariEngine.Logging;
using Microsoft.Xna.Framework;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;

public class Room
{
    public Coord Position { get; private set; }
    public Coord Size { get; private set; }
    public int Distance { get; private set; }
    public RoomFlags Flags { get; private set; }

    public Dictionary<PointOfInterestType, List<PointOfInterest>> PointsOfInterest { get; private init; }
    
    public Vector2 Center => (Vector2)Position + (Vector2)Size / 2;
    
    public Dictionary<Coord, SubRoom> SubRooms { get; private init; }
    
    [YamlIgnore] public HashSet<SubRoomConnection> Connections { get; internal set; }

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

    // ReSharper disable once UnusedMember.Global (YAML serialization)
    public Room()
    {
        
    }
    
    public Room(Coord position, Coord size, int distance, RoomFlags flags = RoomFlags.None)
    {
        Position = position;
        Size = size;
        Distance = distance;
        Flags = flags;

        Connections = [];
        SubRooms = new Dictionary<Coord, SubRoom>();
        for (int y = position.Y; y < position.Y + size.Y; y++)
        {
            for (int x = position.X; x < position.X + size.X; x++)
            {
                Coord subRoomPos = new(x, y);
                SubRooms.Add(subRoomPos, new SubRoom(this, subRoomPos));
            }
        }

        PointsOfInterest = new Dictionary<PointOfInterestType, List<PointOfInterest>>();
    }
    
    public void Connect(AttachNode node, Room otherRoom)
    {
        Coord subRoomPos = node.Position - (Coord)node.Direction;
        SubRoom subRoom = SubRooms[subRoomPos];
        SubRoom newSubRoom = otherRoom.SubRooms[node.Position];

        SubRoomConnection connection = new(subRoom, newSubRoom, node.Direction);

        int count = Connections.Count;
        Connections.Add(connection);
        if (count == Connections.Count)
            Logger.LogDebug("Duplicate connection added");
        otherRoom.Connections.Add(connection.Reversed);
    }

    public void AddPointOfInterest(PointOfInterest poi)
    {
        if (!PointsOfInterest.ContainsKey(poi.PoiType))
            PointsOfInterest[poi.PoiType] = [];
        PointsOfInterest[poi.PoiType].Add(poi);
    }

    public void AddPointOfInterest(PointOfInterestType type, Coord coord) =>
        AddPointOfInterest(new PointOfInterest(type, coord));

    public class YamlConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return type == typeof(Room);
        }

        public object ReadYaml(IParser parser, Type type)
        {
            throw new NotImplementedException();
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            var room = (Room)value!;
            emitter.Emit(new MappingStart());
            
            emitter.Emit(new Scalar(nameof(room.Position)));
            emitter.Emit(new Scalar(room.Position.ToString()));
            
            emitter.Emit(new Scalar(nameof(room.Size)));
            emitter.Emit(new Scalar(room.Size.ToString()));
            
            emitter.Emit(new Scalar(nameof(room.Distance)));
            emitter.Emit(new Scalar(room.Distance.ToString()));
            
            emitter.Emit(new Scalar(nameof(room.Flags)));
            emitter.Emit(new Scalar(room.Flags.ToString()));
            
            emitter.Emit(new Scalar(nameof(room.PointsOfInterest)));
            emitter.Emit(new MappingStart());
            foreach (var (poiType, poiList) in room.PointsOfInterest)
            {
                emitter.Emit(new Scalar(poiType.ToString()));
                emitter.Emit(new SequenceStart(null, null, true, SequenceStyle.Any));
                foreach (var poi in poiList)
                {
                    emitter.Emit(new Scalar(poi.Position.ToString()));
                }
                emitter.Emit(new SequenceEnd());
            }
            emitter.Emit(new MappingEnd());
            
            emitter.Emit(new Scalar(nameof(room.SubRooms)));
            emitter.Emit(new SequenceStart(null, null, true, SequenceStyle.Any));
            foreach (var (pos, subRoom) in room.SubRooms)
            {
                emitter.Emit(new Scalar(pos.ToString()));
            }
            emitter.Emit(new SequenceEnd());
            
            emitter.Emit(new MappingEnd());
        }
    }
}