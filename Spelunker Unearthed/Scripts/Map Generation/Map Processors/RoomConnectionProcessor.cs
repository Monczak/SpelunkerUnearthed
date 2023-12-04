using System.Collections.Generic;
using System.Linq;
using MariEngine.Logging;
using MariEngine.Tiles;
using SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;

namespace SpelunkerUnearthed.Scripts.MapGeneration.MapProcessors;

public class RoomConnectionProcessor : IMapProcessor
{
    public void ProcessMap(Tilemap map, List<Room> rooms)
    {
        var connections = rooms
            .SelectMany(room => room.Connections)
            .DistinctBy(conn => conn, new SubRoomConnectionBidirectionalEqualityComparer());
        foreach (var connection in connections)
        {
            HandleConnection(connection);
        }       
    }

    private void HandleConnection(SubRoomConnection connection)
    {
        Logger.LogDebug($"Connection from {connection.From.Position} to {connection.To.Position}");
    }
}