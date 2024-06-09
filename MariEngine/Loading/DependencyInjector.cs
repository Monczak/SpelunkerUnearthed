using System;
using System.Collections.Generic;

namespace MariEngine.Loading;

public class DependencyInjector
{
    private Dictionary<Type, object> injections = new();

    public DependencyInjector AddDependency(object dependency)
    {
        injections.Add(dependency.GetType(), dependency);
        return this;
    }

    public T GetDependency<T>() where T : class => injections[typeof(T)] as T;
}