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
        var random = ServiceRegistry.Get<RandomProvider>().RequestPositionBased(Constants.MapGenRng);
        Coord spawnPoint = Coord.Zero;

        List<Coord> coordList = [..roomMap.Coords];
        while (coordList.Count > 0)
        {
            int i = random.WithPosition(room.Position).Next(coordList.Count);
            Coord coord = coordList[i];
            coordList.RemoveAt(i);

            // TODO: Proper spawnable space handling
            if (!Tags.HasTag(roomMap[coord].Tags, "Wall"))
            {
                spawnPoint = coord;
                break;
            }
        }
        
        room.AddPointOfInterest(PointOfInterestType.PlayerSpawnPoint, spawnPoint);
    }
}