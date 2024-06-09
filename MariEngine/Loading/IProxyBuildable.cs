namespace MariEngine.Loading;

public interface IProxyBuildable<in TData>
{
    void Build(TData data);
}