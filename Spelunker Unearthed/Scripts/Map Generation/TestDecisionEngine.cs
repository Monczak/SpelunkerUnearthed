using System;
using System.Collections.Generic;
using MariEngine;
using MariEngine.Logging;
using MariEngine.Utils;
using Microsoft.Xna.Framework;
using SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;

namespace SpelunkerUnearthed.Scripts.MapGeneration;

public class TestDecisionEngine : RoomDecisionEngine
{
    protected override Coord MinRoomSize => new(1, 1);
    protected override Coord MaxRoomSize => new(4, 4);
    
    private bool IsHallway(Room room)
    {
        int larger = Math.Max(room.Size.X, room.Size.Y);
        int smaller = Math.Min(room.Size.X, room.Size.Y);
        return (float)larger / smaller >= 2;
    }

    
    protected override float GetSizeWeight(Room sourceRoom, Coord newRoomSize)
    {
        float weight = 10;
        weight *= MathF.Abs(newRoomSize.X - newRoomSize.Y) + 1;
        if (newRoomSize.X != 1 && newRoomSize.Y != 1)
        {
            if (!IsHallway(sourceRoom))
                weight *= 0.1f;
        }
        else 
            weight *= 0.5f;
        
        return weight;
    }

    public override float GetPlacementWeight(Room sourceRoom, (Coord pos, AttachNode node) placement, Coord newRoomSize, List<Room> allRooms)
    {
        float distanceScore = 1;
        if (IsHallway(sourceRoom) && allRooms.Count > 5)
        {
            float minDistance = float.PositiveInfinity;
            foreach (Room room in allRooms)
            {
                if (room == sourceRoom) continue;

                float length = ((Vector2)placement.node.Position - room.Center).LengthSquared();
                minDistance = length < minDistance ? length : minDistance;
            }

            distanceScore = minDistance < 5 ? 0 : minDistance * minDistance;
        }
        
        // Logger.Log(distanceScore);
        
        float hallwayPlacementScore = 1;
        if (IsHallway(sourceRoom))
        {
            float angleCos = MathUtils.AngleCosine(sourceRoom.Size.Direction.ToVector2(),
                placement.node.Direction.ToVector2());
            // Logger.LogDebug($"Source size: {sourceRoom.Size} Placement dir: {placement.node.Direction} Cos: {angleCos}");
            if (angleCos == 0)
                hallwayPlacementScore = 0;
        }
        
        float angleScore = 1;
        if (newRoomSize != Coord.One)
        {
            float angle = MathF.Abs(MathUtils.AngleCosine((Vector2)newRoomSize, placement.node.Direction.ToVector2()));
            angleScore = 1 / (angle * angle + 1);
            angleScore = angleScore * angleScore * angleScore;
        }
        
        // Logger.Log($"Size: {newRoomSize} Direction: {placement.node.Direction} Dot: {dot}");

        return hallwayPlacementScore * angleScore * distanceScore;
    }

    public override float GetBranchingProbability(Room sourceRoom)
    {
        if ((sourceRoom.Flags & RoomFlags.Entrance) != 0)
            return 0.7f;
        return IsHallway(sourceRoom) ? 0.4f : 0;
    }

    public override float GetContinueProbability(Room newRoom)
    {
        return newRoom.Distance < 6 ? 1 : 0;
    }

    public override bool ShouldRegenerate(CaveSystemLevel level)
    {
        return level.Rooms.Count is < 15 or > 50;
    }
}