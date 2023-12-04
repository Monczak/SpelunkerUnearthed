using System;
using System.Collections.Generic;
using System.Linq;
using MariEngine;
using MariEngine.Debugging;
using MariEngine.Logging;
using MariEngine.Tiles;
using Microsoft.Xna.Framework;
using SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;
using SpelunkerUnearthed.Scripts.Utils;

namespace SpelunkerUnearthed.Scripts.MapGeneration.MapProcessors;

public class RoomConnectionProcessor : MapProcessor
{
    private Gizmos gizmos;
    
    public RoomConnectionProcessor(int baseRoomSize, Gizmos gizmos) : base(baseRoomSize)
    {
        this.gizmos = gizmos;
    }
    
    public override void ProcessMap(Tilemap tilemap, CaveSystemLevel level)
    {
        var connections = level.Rooms
            .SelectMany(room => room.Connections)
            .DistinctBy(conn => conn, new SubRoomConnectionBidirectionalEqualityComparer());
        foreach (var connection in connections)
        {
            HandleConnection(tilemap, level, connection);
        }       
    }

    private void HandleConnection(Tilemap tilemap, CaveSystemLevel level, SubRoomConnection connection)
    {
        Logger.LogDebug($"Connection from {connection.From.Position} to {connection.To.Position}");

        Coord fromPoint = RoomMath.TransformRoomPos(level, connection.From.Position, BaseRoomSize) + Coord.One * BaseRoomSize / 2;
        Coord toPoint = RoomMath.TransformRoomPos(level, connection.To.Position, BaseRoomSize) + Coord.One * BaseRoomSize / 2;
        Coord midpoint = (fromPoint + toPoint) / 2;
        Logger.LogDebug($"Midpoint: {midpoint}");

        var (from, to) = FindConnectionCoords(tilemap, level, connection.From.Room, connection.To.Room, midpoint);
        Logger.LogDebug($"From: {from} To: {to}");
        
        gizmos.DrawLine(tilemap.CoordToWorldPoint(from) + Vector2.One * 0.5f, tilemap.CoordToWorldPoint(to) + Vector2.One * 0.5f, Color.Coral, lifetime: 10000);
    }

    private (Coord coord1, Coord coord2) FindConnectionCoords(Tilemap tilemap, CaveSystemLevel level, Room room1,
        Room room2, Coord refCoord) =>
        (GetNearestNothingTilePos(tilemap, level, room1, refCoord), GetNearestNothingTilePos(tilemap, level, room2, refCoord));

    private Coord GetNearestNothingTilePos(Tilemap tilemap, CaveSystemLevel level, Room room, Coord refCoord)
    {
        var bounds = GetRoomBoundsOnTilemap(level, room);
        
        return bounds.Coords
            .Where(c => !Tags.HasTag(tilemap.Get(c, Tilemap.BaseLayer).Tags, "Wall"))
            .MinBy(c => (refCoord - c).SqrMagnitude);
    }

    private CoordBounds GetRoomBoundsOnTilemap(CaveSystemLevel level, Room room) =>
        CoordBounds.MakeCorners(
            RoomMath.RoomPosToTilemapPos(level, room, Coord.Zero, BaseRoomSize),
            RoomMath.RoomPosToTilemapPos(level, room, room.Size * BaseRoomSize, BaseRoomSize) - Coord.One
        );
}