using MariEngine.Logging;

namespace MariEngine.Loading;

public class PackedScene : Resource<SceneData>
{
    public SceneData SceneData { get; private set; }
    
    protected internal override void BuildFromData(SceneData data)
    {
        SceneData = data;
    }
}