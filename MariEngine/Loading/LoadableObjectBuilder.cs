namespace MariEngine.Loading;

public static class LoadableObjectBuilder
{
    public static TObj Build<TObj, TProxyData>(string id, TProxyData data) where TObj : LoadableObject<TProxyData>, new()
    {
        TObj obj = new()
        {
            Id = id
        };
        obj.BuildFromData(data);
        return obj;
    }
}