using MariEngine;

namespace SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;

public class SubRoomConnection
{
    public SubRoom From { get; }
    public SubRoom To { get; }
    
    public Direction Direction { get; }

    public SubRoomConnection Reversed => new(To, From, Direction.Reversed());

    public SubRoomConnection(SubRoom from, SubRoom to, Direction direction)
    {
        From = from;
        To = to;
        Direction = direction;
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