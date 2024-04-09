using System;
using System.Collections.Generic;
using MariEngine.Tiles;
using Microsoft.Xna.Framework;

namespace MariEngine.Utils;

public static class Raycasting
{
    public readonly struct HitInfo
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
        public Vector2 Origin { get; } = origin;
        public Vector2 Direction { get; } = direction.Normalized();
    }

    public static HitInfo? Raycast(TileBuffer buffer, Vector2 origin, Vector2 direction, float maxDistance = 100.0f)
    {
        var debugPoints = new List<Vector2>();

        var dirSign = new Coord(direction.X > 0 ? 1 : -1, direction.Y > 0 ? 1 : -1);
        var tileOffset = new Coord(direction.X > 0 ? 1 : 0, direction.Y > 0 ? 1 : 0);

        var pos = origin;
        var tilePos = (Coord)origin;

        var t = 0f;
        
        while (t < maxDistance)
        {
            var dtX = direction.X == 0 ? float.PositiveInfinity : (tilePos.X + tileOffset.X - pos.X) / direction.X;
            var dtY = direction.Y == 0 ? float.PositiveInfinity : (tilePos.Y + tileOffset.Y - pos.Y) / direction.Y;

            float dt;
            var dTile = Coord.Zero;

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
            
            var c = (Coord)(pos + direction * 0.0001f);
            if (!buffer.IsInBounds(c))
                continue;
            
            debugPoints.Add(pos);
            
            if (buffer[c] is not null && buffer[c].Id != "Nothing")
            {
                var normal = MathUtils.DiamondAngle(pos - ((Vector2)c + Vector2.One * 0.5f)) switch
                {
                    < 0.5f or >= 3.5f => Vector2.UnitX,
                    >= 0.5f and < 1.5f => Vector2.UnitY,
                    >= 1.5f and < 2.5f => -Vector2.UnitX,
                    >= 2.5f and < 3.5f => -Vector2.UnitY,
                    _ => throw new ArgumentOutOfRangeException()
                };
                
                return new HitInfo
                {
                    Position = pos,
                    Tile = buffer[c],
                    DebugPoints = debugPoints.ToArray(),
                    Distance = (pos - origin).Length(),
                    Normal = normal
                };
            }
        }

        return null;
    }
    
    public static HitInfo? Raycast(TileBuffer buffer, Ray ray, float maxDistance = 100.0f)
    {
        return Raycast(buffer, ray.Origin, ray.Direction, maxDistance);
    }
}