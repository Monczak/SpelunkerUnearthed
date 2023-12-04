using System.Collections.Generic;

namespace SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;

public class SubRoomConnectionBidirectionalEqualityComparer : IEqualityComparer<SubRoomConnection>
{
    public bool Equals(SubRoomConnection x, SubRoomConnection y)
    {
        if (x?.From == y?.From && x?.To == y?.To)
            return true;
        
        if (x?.From == y?.To && x?.From == y?.To)
            return true;

        return false;
    }

    public int GetHashCode(SubRoomConnection obj)
    {
        return obj.GetHashCode();
    }
}