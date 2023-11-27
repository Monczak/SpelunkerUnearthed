using System.Collections.Generic;
using System.Linq;
using MariEngine;
using MariEngine.Services;
using MariEngine.Tiles;
using MariEngine.Utils;
using SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;

namespace SpelunkerUnearthed.Scripts.MapGeneration.MapProcessors;

public class PlayerSpawnPointProcessor : MapProcessor
{
    public override void ProcessMap(TileBuffer map, Room room)
    {
        Random random = ServiceRegistry.Get<RandomProvider>().Request(Constants.MapGenRng);
        Coord spawnPoint = Coord.Zero;

        List<Coord> coordList = new(map.Coords);
        while (coordList.Count > 0)
        {
            int i = random.Next(coordList.Count);
            Coord coord = coordList[i];
            coordList.RemoveAt(i);

            // TODO: Proper spawnable space handling
            if (!Tags.HasTag(map[coord].Tags, "Wall"))
            {
                spawnPoint = coord;
                break;
            }
        }
        
        room.AddPointOfInterest(PointOfInterestType.PlayerSpawnPoint, spawnPoint);
    }
}