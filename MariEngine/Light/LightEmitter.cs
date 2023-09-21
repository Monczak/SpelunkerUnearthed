using MariEngine.Components;

namespace MariEngine.Light;

public class LightEmitter : TileEntityComponent
{
    public LightSource Light { get; init; }

    protected override void OnAttach()
    {
        Light.AttachTilemap(OwnerEntity.Tilemap);
        OwnerEntity.Tilemap.GetComponent<LightMap>().AddEmitter(this);
    }

    protected override void OnDestroy()
    {
        OwnerEntity.Tilemap.GetComponent<LightMap>().RemoveEmitter(this);
    }
}