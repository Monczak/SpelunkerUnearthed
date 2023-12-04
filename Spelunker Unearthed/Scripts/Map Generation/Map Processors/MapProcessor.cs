using System.Collections.Generic;
using MariEngine.Tiles;
using SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;

namespace SpelunkerUnearthed.Scripts.MapGeneration.MapProcessors;

public abstract class MapProcessor
{
    protected int BaseRoomSize { get; }
    
    public abstract void ProcessMap(Tilemap map, CaveSystemLevel level);

    public MapProcessor(int baseRoomSize)
    {
        BaseRoomSize = baseRoomSize;
    }
}