using System.Linq;
using MariEngine;
using MariEngine.Components;
using MariEngine.Debugging;
using MariEngine.Input;
using MariEngine.Logging;
using MariEngine.Services;
using MariEngine.Tiles;
using MariEngine.Utils;
using Microsoft.Xna.Framework;
using SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;
using SpelunkerUnearthed.Scripts.Utils;

namespace SpelunkerUnearthed.Scripts.MapGeneration;

public class CaveSystemManager : Component
{
    public CaveSystem CaveSystem { get; } = new();

    private Tilemap tilemap;

    public CaveSystemLevel CurrentLevel { get; set; }

    public void Generate()
    {
        // TODO: Add option for setting the seed
        // ServiceRegistry.Get<RandomNumberGenerator>().Seed(1337);
        CaveSystem.Generate();
    }

    public void SetCurrentLevel(int level)
    {
        if (level < 0 || level >= CaveSystem.Levels.Count)
            Logger.LogError(
                $"Trying to set cave system level to {level}, but deepest level is {CaveSystem.Levels.Count - 1}");
        else
            CurrentLevel = CaveSystem.Levels[level];
    }
}