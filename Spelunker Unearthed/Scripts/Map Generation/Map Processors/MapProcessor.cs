using MariEngine.Components;
using MariEngine.Tiles;
using SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;

namespace SpelunkerUnearthed.Scripts.MapGeneration.MapProcessors;

public abstract class MapProcessor
{
    public abstract void ProcessMap(TileBuffer map, Room room);
}