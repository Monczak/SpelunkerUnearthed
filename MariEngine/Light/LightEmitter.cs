using MariEngine.Components;

namespace MariEngine.Light;

public class LightEmitter : TileEntityComponent<LightEmitterData>
{
    public LightSource LightSource { get; set; }
    private LightMap lightMap;

    public override void Build(LightEmitterData data)
    {
        LightSource = data.LightSource;
    }

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

public readonly record struct LightEmitterData(LightSource LightSource);