using System.Collections.Generic;
using SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;

namespace SpelunkerUnearthed.Scripts.MapGeneration.MapProcessors;

public interface IRoomLayoutProcessor
{
    void ProcessRooms(CaveSystemLevel level);
}