using System.Collections.Generic;

namespace SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;

public class CaveSystem
{
    public List<CaveSystemLevel> Levels { get; private set; } = new();

    public void Generate()
    {
        Levels.Add(new CaveSystemLevel());
        foreach (var level in Levels)
        {
            level.Generate();
        }
    }
}