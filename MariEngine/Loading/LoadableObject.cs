using System;

namespace MariEngine.Loading;

public abstract class LoadableObject<TData>
{
    public string Id { get; protected internal set; }

    protected LoadableObject() { }

    internal T Build<T>(string id, TData data) where T : LoadableObject<TData>, new()
    {
        Id = id;
        BuildFromData(data);
        return (T)this;
    }

    protected internal abstract void BuildFromData(TData data);
}