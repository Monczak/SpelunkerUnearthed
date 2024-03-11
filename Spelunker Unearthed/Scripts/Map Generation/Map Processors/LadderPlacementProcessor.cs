using System.Collections.Generic;
using System.Linq;
using MariEngine;
using MariEngine.Logging;
using MariEngine.Services;
using MariEngine.Tiles;
using SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;
using SpelunkerUnearthed.Scripts.MapGeneration.ParameterProviders;

namespace SpelunkerUnearthed.Scripts.MapGeneration.MapProcessors;

public class LadderPlacementProcessor : IRoomMapProcessor
{
    private static void PlaceLadderPoi(TileBuffer roomMap, Room room, bool isDown)
    {
        var random = ServiceRegistry.Get<RandomProvider>()
            .RequestPositionBased(Constants.MapGenRng)
            .WithPosition(room.Position);

        var poiLocations = room.PointsOfInterest.Values
            .SelectMany(poiList => poiList)
            .Select(poi => poi.Position)
            .ToHashSet();
        
        const int MaxPlacementAttempts = 10;
        for (int i = 0; i < MaxPlacementAttempts; i++)
        {
            var picked = MapProcessorUtil.PickRandomCoord(
                random, 
                roomMap,
                coord => !Tags.HasTag(roomMap[coord].Tags, "Wall") && !poiLocations.Contains(coord),
                out var ladderPoint);    // TODO: Proper spawnable space handling

            if (picked)
            {
                room.AddPointOfInterest(isDown ? PointOfInterestType.LadderDown : PointOfInterestType.LadderUp, ladderPoint);
                
                Logger.LogDebug($"Placed ladder going {(isDown ? "down" : "up")} at {ladderPoint} in room {room.Position}");
                return;
            }
        }

        throw new MapGenerationException($"Could not pick ladder position in room {room.Position}");
    }
    
    public void ProcessRoomMap(TileBuffer roomMap, Room room)
    {
        // Logger.LogDebug($"Room {room.Position}: {room.Flags}");
        if ((room.Flags & (RoomFlags.LadderRoom | RoomFlags.Entrance)) == 0) return;
        
        if ((room.Flags & RoomFlags.LadderRoom) != 0)
            PlaceLadderPoi(roomMap, room, isDown: true);

        if ((room.Flags & RoomFlags.Entrance) != 0)
            PlaceLadderPoi(roomMap, room, isDown: false);
    }
}