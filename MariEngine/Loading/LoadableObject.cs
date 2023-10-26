using System;

namespace MariEngine.Loading;

public class LoadableObject<TData>
{
    public string Id { get; protected internal set; }

    protected LoadableObject() { }

    public LoadableObject(string id)
    {
        Id = id;
    }

    internal T Build<T>(string id, TData data) where T : LoadableObject<TData>, new()
    {
        Id = id;
        BuildFromData(data);
        return (T)this;
    }

    internal virtual void BuildFromData(TData data)
    {
        
    }
}