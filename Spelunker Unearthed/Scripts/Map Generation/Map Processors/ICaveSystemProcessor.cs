using SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;

namespace SpelunkerUnearthed.Scripts.MapGeneration.MapProcessors;

public interface ICaveSystemProcessor
{
    void ProcessCaveSystem(CaveSystem caveSystem);
}