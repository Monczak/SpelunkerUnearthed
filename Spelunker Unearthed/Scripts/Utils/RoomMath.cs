using MariEngine;
using SpelunkerUnearthed.Scripts.MapGeneration;
using SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;

namespace SpelunkerUnearthed.Scripts.Utils;

public static class RoomMath
{
    public static Coord RoomPosToTilemapPos(CaveSystemLevel level, Room room, Coord pos)
    {
        Coord boundsTopLeft = level.BoundingBox.TopLeft;
        return pos - (boundsTopLeft - room.Position) * level.BaseRoomSize;
    }

    public static Coord TilemapPosToRoomPos(CaveSystemLevel level, Room room, Coord pos)
    {
        return pos - RoomPosToTilemapPos(level, room, Coord.Zero);
    }

    public static Coord TransformRoomPos(CaveSystemLevel level, Coord pos)
    {
        Coord boundsTopLeft = level.BoundingBox.TopLeft;
        return (pos - boundsTopLeft) * level.BaseRoomSize;
    }
    
    
    public static CoordBounds GetRoomBounds(CaveSystemLevel level, Room room)
    {
        return CoordBounds.MakeCorners(RoomPosToTilemapPos(level, room, Coord.Zero),
            RoomPosToTilemapPos(level, room, room.Size * level.BaseRoomSize - Coord.One));
    }
}