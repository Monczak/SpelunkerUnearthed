using System;
using System.Collections.Generic;
using MariEngine.Tiles;
using Microsoft.Xna.Framework;

namespace MariEngine.Utils;

public static class Raycasting
{
    public struct HitInfo
    {
        public Vector2 Position { get; init; }
        public Vector2 Normal { get; init; }
        public float Distance { get; init; }
        public Tile Tile { get; init; }
        
        public Vector2[] DebugPoints { get; init; }

        public override string ToString()
        {
            return $"{Position} (normal {Normal}), distance {Distance}, tile {Tile.Id}";
        }
    }

    public readonly struct Ray(Vector2 origin, Vector2 direction)
    {
        public Vector2 Origin { get; init; } = origin;
        public Vector2 Direction { get; init; } = direction.Normalized();
    }

    public static HitInfo? Raycast(TileBuffer buffer, Vector2 origin, Vector2 direction, float maxDistance = 100.0f)
    {
        // TODO: Implement proper raycasting instead of Bresenham and hit normal calculation
        // https://theshoemaker.de/posts/ray-casting-in-2d-grids

        var debugPoints = new List<Vector2>();

        var dirSign = new Coord(direction.X > 0 ? 1 : -1, direction.Y > 0 ? 1 : -1);
        var tileOffset = new Coord(direction.X > 0 ? 1 : 0, direction.Y > 0 ? 1 : 0);

        var pos = origin;
        var tilePos = (Coord)origin;

        var t = 0f;

        // TODO: Add normal calculation
        while (t < maxDistance)
        {
            var dtX = direction.X == 0 ? float.PositiveInfinity : (tilePos.X + tileOffset.X - pos.X) / direction.X;
            var dtY = direction.Y == 0 ? float.PositiveInfinity : (tilePos.Y + tileOffset.Y - pos.Y) / direction.Y;

            float dt;
            var dTile = new Coord(0, 0);

            if (dtX < dtY)
            {
                dt = dtX;
                dTile.X = dirSign.X;
            }
            else
            {
                dt = dtY;
                dTile.Y = dirSign.Y;
            }

            t += dt;
            tilePos += dTile;

            pos += direction * dt;
            
            var c = (Coord)(pos + direction * 0.01f);
            if (!buffer.IsInBounds(c))
                continue;
            
            debugPoints.Add(pos);
            
            if (buffer[c] is not null && buffer[c].Id != "Nothing")
            {
                return new HitInfo
                {
                    Position = pos,
                    Tile = buffer[c],
                    DebugPoints = debugPoints.ToArray(),
                    Distance = t
                };
            }
        }
        
        // foreach (var coord in DrawingUtils.BresenhamLine((Coord)origin,
        //              (Coord)(origin + direction * maxDistance)))
        // {
        //     if (!buffer.IsInBounds(coord))
        //         continue;
        //
        //     debugPoints.Add((Vector2)coord);
        //
        //     var tile = buffer[coord];
        //     if (tile is not null && tile.Id != "Nothing")
        //     {
        //         return new HitInfo
        //         {
        //             Position = coord,
        //             Distance = ((Vector2)coord - origin).Length(),
        //             Tile = tile,
        //             DebugPoints = debugPoints.ToArray()
        //         };
        //     }
        // }

        return null;
    }
    
    public static HitInfo? Raycast(TileBuffer buffer, Ray ray, float maxDistance = 100.0f)
    {
        return Raycast(buffer, ray.Origin, ray.Direction, maxDistance);
    }
}