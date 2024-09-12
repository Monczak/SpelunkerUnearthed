using MariEngine;
using MariEngine.Components;
using MariEngine.Events;
using MariEngine.Logging;
using MariEngine.Services;

namespace SpelunkerUnearthed.Scripts.Managers;

public class GameplayManager(WorldManager worldManager) : Component
{
    protected override void Initialize()
    {
        base.Initialize();
        
        ServiceRegistry.Get<EventManager>().Bind(this, "TriggerWarp", TriggerWarp);
    }

    private void TriggerWarp(Coord fromPosition)
    {
        var currentLevel = worldManager.CaveSystemManager.CurrentLevel;
        if (!currentLevel.MapWarps.TryGetValue(fromPosition, out var warp))
        {
            Logger.LogWarning($"No warp specified for tile {fromPosition}!");
            return;
        }

        Logger.LogDebug($"Warping using {warp}");
        var newLevel = worldManager.CaveSystemManager.CaveSystem.Levels[warp.ToLevel];
        worldManager.StartLoadLevelTask(newLevel)
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Logger.LogError($"Failed to load level {warp}: {task.Exception}");
                    return;
                }

                worldManager.SpawnPlayerFromWarp(warp);
            });
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        ServiceRegistry.Get<EventManager>().UnbindAll(this);
    }
}