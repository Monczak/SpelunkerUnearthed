using MariEngine.Components;

namespace MariEngine.Light;

public class LightEmitter : TileEntityComponent
{
    public LightSource LightSource { get; init; }
    private LightMap lightMap;

    protected internal override void Initialize()
    {
        lightMap = OwnerEntity.Tilemap.GetComponent<LightMap>();
        lightMap.AddEmitter(this);
    }

    protected override void OnDestroy()
    {
        lightMap.RemoveEmitter(this);
    }

    protected internal override void OnPositionUpdate()
    {
        lightMap.UpdatePosition(LightSource, OwnerEntity.Position);
    }
}