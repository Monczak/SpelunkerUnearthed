using System.Collections.Generic;
using MariEngine.Tiles;
using SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;

namespace SpelunkerUnearthed.Scripts.MapGeneration.MapProcessors;

public interface IMapProcessor
{
    void ProcessMap(Tilemap map, List<Room> rooms);
}