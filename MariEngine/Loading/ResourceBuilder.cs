namespace MariEngine.Loading;

public static class ResourceBuilder
{
    public static TObj Build<TObj, TProxyData>(string id, TProxyData data) where TObj : Resource<TProxyData>, new()
    {
        TObj obj = new()
        {
            Id = id
        };
        obj.BuildFromData(data);
        return obj;
    }
}