using System;
using System.Collections.Generic;
using System.Linq;
using MariEngine;
using MariEngine.Debugging;
using MariEngine.Logging;
using MariEngine.Tiles;
using MariEngine.Utils;
using Microsoft.Xna.Framework;
using SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;
using SpelunkerUnearthed.Scripts.Utils;

namespace SpelunkerUnearthed.Scripts.MapGeneration.MapProcessors;

public class RoomConnectionProcessor(int baseRoomSize, Gizmos gizmos) : MapProcessor(baseRoomSize)
{
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
        Coord fromPoint = RoomMath.TransformRoomPos(level, connection.From.Position, BaseRoomSize) + Coord.One * BaseRoomSize / 2;
        Coord toPoint = RoomMath.TransformRoomPos(level, connection.To.Position, BaseRoomSize) + Coord.One * BaseRoomSize / 2;
        Coord midpoint = (fromPoint + toPoint) / 2;

        var (from, to) = FindConnectionCoords(tilemap, level, connection.From.Room, connection.To.Room, midpoint);
        (from, to) = StraightenLine(tilemap, from, to, connection.Direction);
        
        gizmos.DrawLine(tilemap.CoordToWorldPoint(from) + Vector2.One * 0.5f, tilemap.CoordToWorldPoint(to) + Vector2.One * 0.5f, Color.Coral, lifetime: 10000);
    }

    private (Coord from, Coord to) FindConnectionCoords(Tilemap tilemap, CaveSystemLevel level, Room room1,
        Room room2, Coord refCoord) =>
        (GetNearestNothingTilePos(tilemap, level, room1, refCoord), GetNearestNothingTilePos(tilemap, level, room2, refCoord));

    private Coord GetNearestNothingTilePos(Tilemap tilemap, CaveSystemLevel level, Room room, Coord refCoord)
    {
        var bounds = GetRoomBoundsOnTilemap(level, room);
        
        return bounds.Coords
            .Where(c => !Tags.HasTag(tilemap.Get(c, Tilemap.BaseLayer).Tags, "Wall"))
            .MinBy(c => (refCoord - c).SqrMagnitude);
    }

    private CoordBounds GetRoomBoundsOnTilemap(CaveSystemLevel level, Room room)
    {
        return CoordBounds.MakeCorners(
            RoomMath.RoomPosToTilemapPos(level, room, Coord.Zero, BaseRoomSize),
            RoomMath.RoomPosToTilemapPos(level, room, room.Size * BaseRoomSize, BaseRoomSize) - Coord.One
        );
    }

    private (Coord from, Coord to) StraightenLine(Tilemap tilemap, Coord from, Coord to, Direction connectionDirection, int lookahead = 3)
    {
        Coord delta = to - from;
        float angle = MathUtils.DiamondAngle((Vector2)delta);

        Direction dir = angle switch
        {
            < 0.5f or > 3.5f => Direction.Right,
            >= 0.5f and < 1.5f => Direction.Down,
            >= 1.5f and < 2.5f => Direction.Left,
            >= 2.5f and < 3.5f => Direction.Up,
            _ => throw new ArgumentOutOfRangeException()
        };

        if ((connectionDirection & dir) == 0)
            return (from, to);
        
        CoordBounds bounds = CoordBounds.MakeCorners(Coord.Min(from, to) - Coord.One * lookahead, Coord.Max(from, to) + Coord.One * lookahead);
        return Sweep(bounds, dir);

        Coord NarrowInto(int start, int end, Direction direction, int otherCoord)
        {
            Coord coord = default;
            int step = start < end ? 1 : -1;
            for (int i = start; step < 0 ? i >= end : i <= end; i += step)
            {
                coord = (direction & Direction.Horizontal) != 0 ? new Coord(i, otherCoord) : new Coord(otherCoord, i);
                if (!Tags.HasTag(tilemap.Get(coord, Tilemap.BaseLayer).Tags, "Wall"))
                    return coord;
            }

            return coord;
        }

        (Coord from, Coord to) Sweep(CoordBounds sweepBounds, Direction sweepAxis)
        {
            Coord fromCoord = default, toCoord = default;
            
            bool isVertical = (sweepAxis & Direction.Vertical) != 0;
            int orthoStart = isVertical ? sweepBounds.TopLeft.X : sweepBounds.TopLeft.Y;
            int orthoEnd = isVertical ? sweepBounds.BottomRight.X : sweepBounds.BottomRight.Y;
            int sweepStart = isVertical ? sweepBounds.TopLeft.Y : sweepBounds.TopLeft.X;
            int sweepEnd = isVertical ? sweepBounds.BottomRight.Y : sweepBounds.BottomRight.X;

            int center = isVertical ? sweepBounds.Center.Y : sweepBounds.Center.X;
            
            int minWallCount = int.MaxValue;
            for (int ortho = orthoStart; ortho <= orthoEnd; ortho++)
            {
                int wallCount = 0;
                for (int sweep = sweepStart; sweep <= sweepEnd; sweep++)
                {
                    Coord coord = isVertical ? new Coord(ortho, sweep) : new Coord(sweep, ortho);
                    if (Tags.HasTag(tilemap.Get(coord, Tilemap.BaseLayer).Tags, "Wall"))
                        wallCount++;
                }

                if (wallCount < minWallCount)
                {
                    minWallCount = wallCount;
                    
                    fromCoord = NarrowInto(center, sweepStart, sweepAxis.Reversed(), ortho);
                    toCoord = NarrowInto(center, sweepEnd, sweepAxis.Reversed(), ortho);
                    if ((sweepAxis & (Direction.Down | Direction.Right)) != 0)
                        (fromCoord, toCoord) = (toCoord, fromCoord);
                }
            }

            return (fromCoord, toCoord);
        }
    }
}