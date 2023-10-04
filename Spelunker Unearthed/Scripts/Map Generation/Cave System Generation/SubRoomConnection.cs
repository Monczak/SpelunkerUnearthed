using System.Collections.Generic;
using System.Linq;
using MariEngine.Services;

namespace SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;

public class SubRoomConnection
{
    public SubRoom From { get; }
    public SubRoom To { get; }

    public SubRoomConnection Reversed => new(To, From);

    public SubRoomConnection(SubRoom from, SubRoom to)
    {
        From = from;
        To = to;
    }
    
    public static SubRoomConnection MakeConnection(Room room1, Room room2)
    {
        if (room1 == room2)
            return null;
        
        List<(SubRoom subRoom1, SubRoom subRoom2)> connectionCandidates = 
            (from subRoom in room1.SubRooms.Values 
                from otherSubRoom in room2.SubRooms.Values 
                where subRoom.NextTo(otherSubRoom) 
                select (subRoom, otherSubRoom)).ToList();

        if (connectionCandidates.Count == 0)
            return null;
        
        var (subRoom1, subRoom2) =
            connectionCandidates[ServiceRegistry.Get<RandomProvider>().Request(Constants.CaveSystemGen).Next(connectionCandidates.Count)];
        SubRoomConnection connection = new(subRoom1, subRoom2);
        subRoom1.Room.Connections.Add(connection);
        subRoom2.Room.Connections.Add(connection.Reversed);

        return connection;
    }

    public override bool Equals(object obj)
    {
        if (obj is null || obj.GetType() != GetType())
            return false;
        return Equals((SubRoomConnection)obj);
    }
    
    private bool Equals(SubRoomConnection other) => From == other.From && To == other.To;

    public override int GetHashCode()
    {
        return From.GetHashCode() ^ To.GetHashCode();
    }
}