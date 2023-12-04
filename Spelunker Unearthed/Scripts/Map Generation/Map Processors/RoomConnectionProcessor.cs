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
        (from, to) = StraightenLine(tilemap, from, to, connection.Direction);
        Logger.LogDebug($"From: {from} To: {to}");
        
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
        Logger.LogDebug(dir);
        Logger.LogDebug(connectionDirection);

        if ((connectionDirection & dir) == 0)
            return (from, to);
        
        // TODO: Straighten (sweep a line across the room boundary within the area occupied by bounds with corners (from, to) and find the shortest one
        CoordBounds bounds = CoordBounds.MakeCorners(Coord.Min(from, to) - Coord.One * lookahead, Coord.Max(from, to) + Coord.One * lookahead);
        Logger.LogDebug("Straighten");

        // TODO: Refactor this huge bowl of spaghetti
        int minWallCount = int.MaxValue;        
        if ((dir & Direction.Horizontal) != 0)
        {
            for (int x = bounds.TopLeft.X; x <= bounds.BottomRight.X; x++)
            {
                int wallCount = 0;
                for (int y = bounds.TopLeft.Y; y <= bounds.BottomRight.Y; y++)
                {
                    if (Tags.HasTag(tilemap.Get(new Coord(x, y), Tilemap.BaseLayer).Tags, "Wall"))
                        wallCount++;
                }

                if (wallCount < minWallCount)
                {
                    minWallCount = wallCount;
                    if (dir == Direction.Up)
                    {
                        for (int y = bounds.Center.Y; y >= bounds.TopLeft.Y; y--)
                        {
                            Coord coord = new(x, y);
                            if (!Tags.HasTag(tilemap.Get(coord, Tilemap.BaseLayer).Tags, "Wall"))
                            {
                                from = coord;
                                break;
                            }
                        }
                        for (int y = bounds.Center.Y; y <= bounds.BottomRight.Y; y++)
                        {
                            Coord coord = new(x, y);
                            if (!Tags.HasTag(tilemap.Get(coord, Tilemap.BaseLayer).Tags, "Wall"))
                            {
                                to = coord;
                                break;
                            }
                        }    
                    }
                    else
                    {
                        for (int y = bounds.Center.Y; y <= bounds.BottomRight.Y; y++)
                        {
                            Coord coord = new(x, y);
                            if (!Tags.HasTag(tilemap.Get(coord, Tilemap.BaseLayer).Tags, "Wall"))
                            {
                                from = coord;
                                break;
                            }
                        }
                        for (int y = bounds.Center.Y; y >= bounds.TopLeft.Y; y--)
                        {
                            Coord coord = new(x, y);
                            if (!Tags.HasTag(tilemap.Get(coord, Tilemap.BaseLayer).Tags, "Wall"))
                            {
                                to = coord;
                                break;
                            }
                        } 
                    }
                }
            }
        }
        else
        {
            for (int y = bounds.TopLeft.Y; y <= bounds.BottomRight.Y; y++)
            {
                int wallCount = 0;
                for (int x = bounds.TopLeft.X; x <= bounds.BottomRight.X; x++)
                {
                    if (Tags.HasTag(tilemap.Get(new Coord(x, y), Tilemap.BaseLayer).Tags, "Wall"))
                        wallCount++;
                }

                if (wallCount < minWallCount)
                {
                    minWallCount = wallCount;
                    if (dir == Direction.Left)
                    {
                        for (int x = bounds.Center.X; x >= bounds.TopLeft.X; x--)
                        {
                            Coord coord = new(x, y);
                            if (!Tags.HasTag(tilemap.Get(coord, Tilemap.BaseLayer).Tags, "Wall"))
                            {
                                from = coord;
                                break;
                            }
                        }

                        for (int x = bounds.Center.X; y <= bounds.BottomRight.X; x++)
                        {
                            Coord coord = new(x, y);
                            if (!Tags.HasTag(tilemap.Get(coord, Tilemap.BaseLayer).Tags, "Wall"))
                            {
                                to = coord;
                                break;
                            }
                        }
                    }
                    else
                    {
                        for (int x = bounds.Center.X; x <= bounds.BottomRight.X; x++)
                        {
                            Coord coord = new(x, y);
                            if (!Tags.HasTag(tilemap.Get(coord, Tilemap.BaseLayer).Tags, "Wall"))
                            {
                                from = coord;
                                break;
                            }
                        }

                        for (int x = bounds.Center.X; x >= bounds.TopLeft.X; x--)
                        {
                            Coord coord = new(x, y);
                            if (!Tags.HasTag(tilemap.Get(coord, Tilemap.BaseLayer).Tags, "Wall"))
                            {
                                to = coord;
                                break;
                            }
                        }
                    }
                }
            }
        }
        
        return (from, to);
    }
}