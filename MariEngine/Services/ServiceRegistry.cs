using System;
using System.Collections.Generic;
using MariEngine.Logging;

namespace MariEngine.Services;

public static class ServiceRegistry
{
    private static Dictionary<Type, Service> services;

    public static void RegisterService<T>(T service) where T : Service
    {
        services ??= new Dictionary<Type, Service>();

        if (services.ContainsKey(typeof(T)))
            throw new ArgumentException($"Service of type {typeof(T).Name} is already registered");
        services[typeof(T)] = service;
        
        Logger.Log($"Registered service {typeof(T).Name}");
    }

    public static T Get<T>() where T : Service
    {
        if (!services.ContainsKey(typeof(T)))
            throw new ArgumentException($"Service of type {typeof(T).Name} has not been registered");
        return (T)services[typeof(T)];
    }

    public static void UpdateServices()
    {
        foreach (Service service in services.Values)
            service.Update();
    }
}