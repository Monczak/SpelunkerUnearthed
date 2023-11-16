using System.Collections.Generic;
using MariEngine.Logging;
using SpelunkerUnearthed.Scripts.MapGeneration.Biomes;

namespace SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;

public class CaveSystem
{
    public List<CaveSystemLevel> Levels { get; private set; } = new();
    public BiomeMap BiomeMap { get; private set; } = new(new SimpleBiomeProvider());

    public RoomDecisionEngine DecisionEngine { get; set; } = new TestDecisionEngine();  // TODO: Remove this assignment

    private const int MaxGenerationAttempts = 10;
    
    public void Generate()
    {
        Levels.Clear();
        
        // TODO: Add levels procedurally
        Levels.Add(new CaveSystemLevel());
        
        for (int i = 0; i < Levels.Count; i++)
        {
            var level = Levels[i];
            int attempt = 0;
            while (attempt < MaxGenerationAttempts)
            {
                level.Generate(DecisionEngine);
                attempt++;

                if (!DecisionEngine.ShouldRegenerate(level))
                    break;
            }

            if (attempt == MaxGenerationAttempts)
                Logger.LogWarning($"Exceeded max generation attempts for level {i}");
        }
    }
}