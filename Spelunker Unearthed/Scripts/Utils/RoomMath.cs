using MariEngine;
using SpelunkerUnearthed.Scripts.MapGeneration;
using SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;

namespace SpelunkerUnearthed.Scripts.Utils;

public static class RoomMath
{
    public static Coord RoomPosToTilemapPos(CaveSystemLevel level, Room room, Coord pos, int baseTilemapSize)
    {
        Coord boundsTopLeft = level.BoundingBox.TopLeft;
        return pos - (boundsTopLeft - room.Position) * baseTilemapSize;
    }

    public static Coord TilemapPosToRoomPos(CaveSystemLevel level, Room room, Coord pos, int baseTilemapSize)
    {
        return pos - RoomPosToTilemapPos(level, room, Coord.Zero, baseTilemapSize);
    }

    public static Coord TransformRoomPos(CaveSystemLevel level, Coord pos, int baseTilemapSize)
    {
        Coord boundsTopLeft = level.BoundingBox.TopLeft;
        return (pos - boundsTopLeft) * baseTilemapSize;
    }
    
    
    public static CoordBounds GetRoomBounds(CaveSystemLevel level, Room room, int baseTilemapSize)
    {
        return CoordBounds.MakeCorners(RoomPosToTilemapPos(level, room, Coord.Zero, baseTilemapSize),
            RoomPosToTilemapPos(level, room, room.Size * baseTilemapSize - Coord.One, baseTilemapSize));
    }
}