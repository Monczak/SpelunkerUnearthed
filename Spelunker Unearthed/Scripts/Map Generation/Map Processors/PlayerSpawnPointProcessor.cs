using System.Collections.Generic;
using System.Linq;
using MariEngine;
using MariEngine.Services;
using MariEngine.Tiles;
using MariEngine.Utils;
using SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;

namespace SpelunkerUnearthed.Scripts.MapGeneration.MapProcessors;

public class PlayerSpawnPointProcessor : IRoomMapProcessor
{
    public void ProcessRoomMap(TileBuffer roomMap, Room room)
    {
        var random = ServiceRegistry.Get<RandomProvider>()
            .RequestPositionBased(Constants.MapGenRng)
            .WithPosition(room.Position);
        
        var picked = MapProcessorUtil.PickRandomCoord(
            random, 
            roomMap, 
            coord => !Tags.HasTag(roomMap[coord].Tags, "Wall"),
            out var spawnPoint);   // TODO: Proper spawnable space handling

        if (!picked)
            throw new MapGenerationException($"Could not determine spawn point in room {room.Position}");
        
        room.AddPointOfInterest(PointOfInterestType.PlayerSpawnPoint, spawnPoint);
    }
}