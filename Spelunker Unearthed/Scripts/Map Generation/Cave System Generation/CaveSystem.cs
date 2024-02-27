using System.Collections.Generic;
using MariEngine.Logging;
using MariEngine.Services;
using SpelunkerUnearthed.Scripts.MapGeneration.Biomes;
using SpelunkerUnearthed.Scripts.MapGeneration.MapProcessors;

namespace SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;

public class CaveSystem(IBiomeProvider biomeProvider, RoomDecisionEngine roomDecisionEngine, IEnumerable<IRoomLayoutProcessor> roomLayoutProcessors)
{
    public List<CaveSystemLevel> Levels { get; private set; } = [];
    public BiomeMap BiomeMap { get; private set; } = new(biomeProvider);

    private const int MaxGenerationAttempts = 10;
    
    public void Generate()
    {
        Levels.Clear();
        
        // TODO: Add levels procedurally
        Levels.Add(new CaveSystemLevel { Depth = 0, MapGenSeed = ServiceRegistry.Get<RandomProvider>().Request(Constants.CaveSystemGenRng).Next()});
        
        for (int i = 0; i < Levels.Count; i++)
        {
            var level = Levels[i];
            int attempt = 0;
            while (attempt < MaxGenerationAttempts)
            {
                level.Generate(roomDecisionEngine, roomLayoutProcessors);
                attempt++;

                if (!roomDecisionEngine.ShouldRegenerate(level))
                    break;
            }

            if (attempt == MaxGenerationAttempts)
                Logger.LogWarning($"Exceeded max generation attempts for level {i}");
        }
    }
}