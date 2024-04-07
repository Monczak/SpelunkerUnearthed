using MariEngine.Tiles;
using Microsoft.Xna.Framework;

namespace MariEngine.Utils;

public static class Raycasting
{
    public struct HitInfo
    {
        public Coord Position { get; init; }
        public Vector2 Normal { get; init; }
        public float Distance { get; init; }
        public Tile Tile { get; init; }

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

    public static HitInfo? Raycast(TileBuffer buffer, Coord pos, Vector2 direction, float maxDistance = 100.0f)
    {
        return Raycast(buffer, new Ray((Vector2)pos, direction), maxDistance);
    }
    
    public static HitInfo? Raycast(TileBuffer buffer, Ray ray, float maxDistance = 100.0f)
    {
        // TODO: Implement proper raycasting instead of Bresenham and hit normal calculation
        // https://theshoemaker.de/posts/ray-casting-in-2d-grids

        foreach (var coord in DrawingUtils.BresenhamLine((Coord)ray.Origin,
                     (Coord)(ray.Origin + ray.Direction * maxDistance)))
        {
            if (!buffer.IsInBounds(coord))
                continue;

            var tile = buffer[coord];
            if (tile is not null && tile.Id != "Nothing")
            {
                return new HitInfo
                {
                    Position = coord,
                    Distance = ((Vector2)coord - ray.Origin).Length(),
                    Tile = tile
                };
            }
        }

        return null;
    }
}