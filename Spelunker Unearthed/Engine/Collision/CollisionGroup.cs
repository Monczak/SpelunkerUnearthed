using System;

namespace SpelunkerUnearthed.Engine.Collision;

[Flags]
public enum CollisionGroup
{
    Group1 = 1 << 0,
    Group2 = 1 << 1,
    Group3 = 1 << 2,
    Group4 = 1 << 3,
    Group5 = 1 << 4,
    Group6 = 1 << 5,
    Group7 = 1 << 6,
    Group8 = 1 << 7,
    
    None = 0,
    All = ~None
}