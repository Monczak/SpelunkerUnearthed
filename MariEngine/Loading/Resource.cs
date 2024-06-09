using System;

namespace MariEngine.Loading;

public abstract class Resource<TData> : IProxyBuildable<TData>
{
    public string Id { get; protected internal set; }

    protected Resource() { }

    protected Resource(string id)
    {
        Id = id;
    }
    
    public void Build(TData data)
    {
        BuildFromData(data);
    }

    protected internal abstract void BuildFromData(TData data);
}