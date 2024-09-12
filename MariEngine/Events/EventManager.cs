using System;
using System.Collections.Generic;
using System.Linq;
using MariEngine.Logging;
using MariEngine.Services;

namespace MariEngine.Events;

public class EventManager : Service
{
    private readonly Dictionary<string, Dictionary<object, List<Delegate>>> handlers = new();
    
    public void Bind(object context, string eventName, Delegate handler)
    {
        if (!handlers.ContainsKey(eventName))
            handlers[eventName] = new Dictionary<object, List<Delegate>>();
        if (!handlers[eventName].ContainsKey(context))
            handlers[eventName][context] = [];
        
        handlers[eventName][context].Add(handler);
    }

    public void Unbind(object context, string eventName, Delegate handler)
    {
        if (handlers.TryGetValue(eventName, out var contextDict))
        {
            if (contextDict.TryGetValue(context, out var delegates))
                delegates.Remove(handler);
        }
    }

    public void UnbindAll(object context)
    {
        foreach (var contextDict in handlers.Values) 
            contextDict.Remove(context);
    }
    
    public void Notify(string eventName, params object[] args)
    {
        if (!handlers.TryGetValue(eventName, out var contextDict))
        {
            Logger.LogWarning($"No handler for event {eventName}!");
            return;
        }

        foreach (var handler in contextDict.Values.SelectMany(handlerList => handlerList))
        {
            handler.DynamicInvoke(args);
        }
    }
}