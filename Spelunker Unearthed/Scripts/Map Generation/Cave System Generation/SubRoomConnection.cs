using MariEngine;

namespace SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;

public class SubRoomConnection
{
    // ReSharper disable once UnusedMember.Global (YAML serialization)
    public SubRoomConnection()
    {
    }

    public SubRoomConnection(SubRoom from, SubRoom to, Direction direction)
    {
        From = from;
        To = to;
        Direction = direction;
    }

    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local (required for YAML serialization)
    public SubRoom From { get; private set; }
    
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local (required for YAML serialization)
    public SubRoom To { get; private set; }

    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local (required for YAML serialization)
    public Direction Direction { get; private set; }

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