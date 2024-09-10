using System;
using System.Collections.Generic;
using System.Linq;
using MariEngine.Exceptions;
using MariEngine.Tiles;

namespace MariEngine.Services;

public class TileBehaviorProvider : Service
{
    private readonly Dictionary<string, TileBehavior> behaviors = new();
    
    public TileBehavior CreateBehavior(string behaviorName)
    {
        var behaviorType = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .FirstOrDefault(t => t.IsSubclassOf(typeof(TileBehavior)) && t.Name == behaviorName && !t.IsAbstract);
        
        if (behaviorType is null)
            throw new ContentLoadingException($"Could not find a tile behavior with the name {behaviorName}.");
        
        if (behaviors.TryGetValue(behaviorName, out var behavior) 
            && !behaviorType.GetCustomAttributes(false).Any(a => a.GetType() == typeof(DontCacheBehaviorAttribute)))
            return behavior;
        
        behavior = Activator.CreateInstance(behaviorType) as TileBehavior;
        behaviors[behaviorName] = behavior;
        return behavior;
    }
}