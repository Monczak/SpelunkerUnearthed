using MariEngine.Components;
using MariEngine.Tiles;
using SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;

namespace SpelunkerUnearthed.Scripts.MapGeneration.MapProcessors;

public interface IRoomMapProcessor
{
    void ProcessRoomMap(TileBuffer roomMap, Room room);
}