using System.Collections.Generic;

namespace SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;

public class CaveSystem
{
    public List<CaveSystemLevel> Levels { get; private set; } = new();

    public RoomDecisionEngine DecisionEngine { get; set; } = new TestDecisionEngine();  // TODO: Remove this assignment

    public void Generate()
    {
        Levels.Clear();
        Levels.Add(new CaveSystemLevel());
        foreach (var level in Levels)
        {
            do
            {
                level.Generate(DecisionEngine);
            } 
            while (DecisionEngine.ShouldRegenerate(level));
        }
    }
}