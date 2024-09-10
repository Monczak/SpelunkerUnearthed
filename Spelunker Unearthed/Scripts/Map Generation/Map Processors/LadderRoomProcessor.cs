using System.Collections.Generic;
using System.Linq;
using MariEngine.Services;
using SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;

namespace SpelunkerUnearthed.Scripts.MapGeneration.MapProcessors;

public class LadderRoomProcessor : IRoomLayoutProcessor
{
    private static float CalculateLadderRoomChance(Room room, int maxDistance)
    {
        return 1f / (maxDistance - room.Distance + 1);
    }
    
    public void ProcessRooms(CaveSystemLevel level)
    {
        // TODO: Better depth limiting (cave system properties maybe)
        if (level.Depth > 4)
            return;
        
        var random = ServiceRegistry.Get<RandomProvider>().Request(Constants.CaveSystemGenRng);
        var maxDistance = level.Rooms.Max(room => room.Distance);

        List<(Room, float)> ladderRoomChances = [];
        foreach (var room in level.Rooms)
        {
            ladderRoomChances.Add((room, CalculateLadderRoomChance(room, maxDistance)));
        }

        var ladderRoom = random.PickWeighted(ladderRoomChances, out _);
        ladderRoom.Flags |= RoomFlags.LadderRoom;
    }
}