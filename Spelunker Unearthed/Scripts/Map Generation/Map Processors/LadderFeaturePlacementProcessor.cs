using System.Collections.Generic;
using MariEngine;
using MariEngine.Debugging;
using MariEngine.Tiles;
using SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;
using SpelunkerUnearthed.Scripts.MapGeneration.Features;
using SpelunkerUnearthed.Scripts.Utils;

namespace SpelunkerUnearthed.Scripts.MapGeneration.MapProcessors;

public class LadderFeaturePlacementProcessor : MapProcessor
{
    public override void ProcessMap(TileBuffer walls, TileBuffer ground, CaveSystemLevel level)
    {
        foreach (var room in level.Rooms)
        {
            var ladderDownPois = room.PointsOfInterest.GetValueOrDefault(PointOfInterestType.LadderDown) ?? [];
            var ladderUpPois = room.PointsOfInterest.GetValueOrDefault(PointOfInterestType.LadderUp) ?? [];
            
            foreach (var poi in ladderDownPois)
                FeaturePlacer.Place(new Ladder(isDown: true), ground, RoomMath.RoomPosToTilemapPos(level, room, poi.Position));
            foreach (var poi in ladderUpPois)
                FeaturePlacer.Place(new Ladder(isDown: false), ground, RoomMath.RoomPosToTilemapPos(level, room, poi.Position));
        }
    }
}