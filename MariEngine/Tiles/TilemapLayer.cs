using System;

namespace MariEngine.Tiles;

[Flags]
public enum TilemapLayer
{
    Ground = 1 << 0,
    Base = 1 << 1,
    
    None = 0,
    All = ~None,
}
