using System;

namespace MariEngine.Loading;

public class Resource<TData>
{
    public string Id { get; protected internal set; }

    protected Resource() { }

    public Resource(string id)
    {
        Id = id;
    }

    internal T Build<T>(string id, TData data) where T : Resource<TData>, new()
    {
        Id = id;
        BuildFromData(data);
        return (T)this;
    }

    protected internal abstract void BuildFromData(TData data);
}