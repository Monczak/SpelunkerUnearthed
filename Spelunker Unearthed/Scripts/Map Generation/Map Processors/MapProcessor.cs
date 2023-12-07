using System.Collections.Generic;
using MariEngine.Tiles;
using SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;

namespace SpelunkerUnearthed.Scripts.MapGeneration.MapProcessors;

public abstract class MapProcessor(int baseRoomSize)
{
    protected int BaseRoomSize { get; } = baseRoomSize;

    public abstract void ProcessMap(Tilemap map, CaveSystemLevel level);
}