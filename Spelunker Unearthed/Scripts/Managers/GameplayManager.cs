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
        
        ServiceRegistry.Get<EventManager>().Bind(this, "ChangeCaveSystemLevel", ChangeCaveSystemLevel);
    }

    private void ChangeCaveSystemLevel(int levelDelta)
    {
        var currentLevel = worldManager.CaveSystemManager.CurrentLevel;
        var newDepth = currentLevel.Depth + levelDelta;

        Logger.LogDebug($"Changing level to {newDepth}");
        var newLevel = worldManager.CaveSystemManager.CaveSystem.Levels[newDepth];
        worldManager.StartLoadLevelTask(newLevel);
        // TODO: This should also properly clean the light map (maybe do this in Tilemap or LightMap itself)
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        ServiceRegistry.Get<EventManager>().UnbindAll(this);
    }
}