using System;

namespace SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;

[Flags]
public enum RoomFlags
{
    Entrance = 1 << 0,
    LadderRoom = 1 << 1,
    
    None = 0,
    All = ~None
}