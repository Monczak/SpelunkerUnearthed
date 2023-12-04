using MariEngine;
using SpelunkerUnearthed.Scripts.MapGeneration;
using SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;

namespace SpelunkerUnearthed.Scripts.Utils;

public static class RoomMath
{
    public static Coord RoomPosToTilemapPos(CaveSystemLevel level, Room room, Coord pos, int baseRoomSize)
    {
        Coord boundsTopLeft = level.BoundingBox.TopLeft;
        return pos - (boundsTopLeft - room.Position) * baseRoomSize;
    }

    public static Coord TilemapPosToRoomPos(CaveSystemLevel level, Room room, Coord pos, int baseRoomSize)
    {
        return pos - RoomPosToTilemapPos(level, room, Coord.Zero, baseRoomSize);
    }

    public static Coord TransformRoomPos(CaveSystemLevel level, Coord pos, int baseRoomSize)
    {
        Coord boundsTopLeft = level.BoundingBox.TopLeft;
        return (pos - boundsTopLeft) * baseRoomSize;
    }
    
    
    public static CoordBounds GetRoomBounds(CaveSystemLevel level, Room room, int baseRoomSize)
    {
        return CoordBounds.MakeCorners(RoomPosToTilemapPos(level, room, Coord.Zero, baseRoomSize),
            RoomPosToTilemapPos(level, room, room.Size * baseRoomSize - Coord.One, baseRoomSize));
    }
}