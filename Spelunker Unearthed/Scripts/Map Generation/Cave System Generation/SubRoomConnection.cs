using MariEngine;

namespace SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;

public class SubRoomConnection(SubRoom from, SubRoom to, Direction direction)
{
    public SubRoom From { get; } = from;
    public SubRoom To { get; } = to;

    public Direction Direction { get; } = direction;

    public SubRoomConnection Reversed => new(To, From, Direction.Reversed());

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