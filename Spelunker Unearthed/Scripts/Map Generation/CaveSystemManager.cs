using System.Collections.Generic;
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
using SpelunkerUnearthed.Scripts.MapGeneration.Biomes;
using SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;
using SpelunkerUnearthed.Scripts.MapGeneration.MapProcessors;
using SpelunkerUnearthed.Scripts.Utils;

namespace SpelunkerUnearthed.Scripts.MapGeneration;

public class CaveSystemManager(IBiomeProvider biomeProvider, RoomDecisionEngine roomDecisionEngine, IEnumerable<IRoomLayoutProcessor> roomLayoutProcessors) : Component
{
    public CaveSystem CaveSystem { get; } = new(biomeProvider, roomDecisionEngine, roomLayoutProcessors);

    private Tilemap tilemap;

    public CaveSystemLevel CurrentLevel { get; set; }

    public void Generate(int worldSeed)
    {
        ServiceRegistry.Get<RandomProvider>().Request(Constants.CaveSystemGenRng).Seed(worldSeed);
        ServiceRegistry.Get<RandomProvider>().Request(Constants.BiomeGenRng).Seed(worldSeed);
        CaveSystem.Generate();
    }

    public CaveSystemLevel SetCurrentLevel(int level)
    {
        if (level < 0 || level >= CaveSystem.Levels.Count)
            Logger.LogError(
                $"Trying to set cave system level to {level}, but deepest level is {CaveSystem.Levels.Count - 1}");
        else
            CurrentLevel = CaveSystem.Levels[level];

        return CurrentLevel;
    }

    public Biome GetBiome(Coord worldPos) => CurrentLevel is null ? null : CaveSystem.BiomeMap.GetBiome(worldPos, CurrentLevel.Depth);
}