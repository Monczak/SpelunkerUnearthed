using System;
using System.Collections.Generic;
using MariEngine;
using MariEngine.Tiles;
using MariEngine.Utils;
using Random = MariEngine.Utils.Random;

namespace SpelunkerUnearthed.Scripts.MapGeneration.MapProcessors;

public static class MapProcessorUtil
{
    public static bool PickRandomCoord(PositionBasedRandom random, TileBuffer roomMap, Func<Coord, bool> filter, out Coord pos)
    {
        pos = Coord.Zero;

        List<Coord> coordList = [..roomMap.Coords];
        while (coordList.Count > 0)
        {
            var i = random.Next(coordList.Count);
            var coord = coordList[i];
            coordList.RemoveAt(i);
            
            if (filter(coord))
            {
                pos = coord;
                return true;
            }
        }

        return false;
    }
    
}