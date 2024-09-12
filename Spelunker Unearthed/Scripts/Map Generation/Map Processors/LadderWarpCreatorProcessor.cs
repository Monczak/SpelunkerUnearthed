using System.Collections.Generic;
using System.Linq;
using MariEngine.Logging;
using SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;
using SpelunkerUnearthed.Scripts.Utils;

namespace SpelunkerUnearthed.Scripts.MapGeneration.MapProcessors;

public class LadderWarpCreatorProcessor : ICaveSystemProcessor
{
    public void ProcessCaveSystem(CaveSystem caveSystem)
    {
        foreach (var level in caveSystem.Levels.Where(level => level.Depth + 1 < caveSystem.Levels.Count)) 
            LinkLadders(level, caveSystem.Levels[level.Depth + 1], true);
        
        foreach (var level in caveSystem.Levels.Where(level => level.Depth - 1 >= 0)) 
            LinkLadders(level, caveSystem.Levels[level.Depth - 1], false);
    }

    private void LinkLadders(CaveSystemLevel currentLevel, CaveSystemLevel destinationLevel, bool down)
    {
        var linkedPois = new HashSet<(Room, PointOfInterest)>();

        var sourceLadderType = down ? PointOfInterestType.LadderDown : PointOfInterestType.LadderUp;
        var destinationLadderType = down ? PointOfInterestType.LadderUp : PointOfInterestType.LadderDown;
            
        foreach (var ladderRoom in currentLevel.Rooms.Where(r => r.PointsOfInterest.ContainsKey(sourceLadderType)))
        {
            foreach (var ladderPoi in ladderRoom.PointsOfInterest[sourceLadderType])
            {
                var (closestSecondLadderRoom, closestSecondLadderPoi) = destinationLevel.Rooms
                    .SelectMany(room => room.PointsOfInterest
                        .Where(pair => pair.Key == destinationLadderType)
                        .SelectMany(pair => pair.Value, (_, poi) => (room, poi))
                    )
                    .Where(pair => !linkedPois.Contains(pair))
                    .MinBy(pair =>
                        (RoomMath.RoomPosToWorldPos(destinationLevel, pair.room, pair.poi.Position) -
                         RoomMath.RoomPosToWorldPos(currentLevel, ladderRoom, ladderPoi.Position))
                        .SqrMagnitude);

                var warp = new MapWarp(
                    currentLevel.Depth,
                    destinationLevel.Depth,
                    RoomMath.RoomPosToTilemapPos(currentLevel, ladderRoom, ladderPoi.Position),
                    RoomMath.RoomPosToTilemapPos(destinationLevel, closestSecondLadderRoom, closestSecondLadderPoi.Position)
                );
                Logger.LogDebug($"Created ladder warp: {warp}");
                currentLevel.AddWarp(warp);
                
                linkedPois.Add((closestSecondLadderRoom, closestSecondLadderPoi));
            }
        }
    }
}