using System;
using System.Collections.Generic;
using MariEngine.Loading;
using MariEngine.Logging;
using Microsoft.Xna.Framework;

namespace MariEngine.Services;

public static class ServiceRegistry
{
    private static Dictionary<Type, Service> services;

    internal static IEnumerable<KeyValuePair<Type, Service>> Services => services;

    public static void RegisterService<T>(T service) where T : Service
    {
        services ??= new Dictionary<Type, Service>();

        if (services.ContainsKey(typeof(T)))
            throw new ArgumentException($"Service of type {typeof(T).Name} is already registered");
        services[typeof(T)] = service;
        
        Logger.Log($"Registered service {typeof(T).Name}");
    }

    public static T Get<T>() where T : Service => (T)Get(typeof(T));

    internal static Service Get(Type type)
    {
        if (!services.TryGetValue(type, out var service))
            throw new ArgumentException($"Service of type {type.Name} has not been registered");
        return service;
    }

    public static void UpdateServices(GameTime gameTime)
    {
        foreach (var service in services.Values)
            service.Update(gameTime);
    }
}