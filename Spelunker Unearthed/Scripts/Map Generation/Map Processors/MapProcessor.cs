using System.Collections.Generic;
using MariEngine.Tiles;
using SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;

namespace SpelunkerUnearthed.Scripts.MapGeneration.MapProcessors;

public abstract class MapProcessor
{
    public abstract void ProcessMap(TileBuffer walls, TileBuffer ground, CaveSystemLevel level);
}