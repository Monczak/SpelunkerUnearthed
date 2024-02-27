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
    
    public void ProcessRooms(IList<Room> rooms)
    {
        var random = ServiceRegistry.Get<RandomProvider>().Request(Constants.CaveSystemGenRng);
        var maxDistance = rooms.Max(room => room.Distance);

        List<(Room, float)> ladderRoomChances = [];
        foreach (var room in rooms)
        {
            ladderRoomChances.Add((room, CalculateLadderRoomChance(room, maxDistance)));
        }

        var ladderRoom = random.PickWeighted(ladderRoomChances, out _);
        ladderRoom.Flags |= RoomFlags.LadderRoom;
    }
}