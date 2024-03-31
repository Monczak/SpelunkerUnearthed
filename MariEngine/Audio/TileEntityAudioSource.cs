using MariEngine.Components;
using Microsoft.Xna.Framework;

namespace MariEngine.Audio;

public class TileEntityAudioSource(AudioEvent audioEvent) : TileEntityComponent
{
    public void Play(Coord? positionOverride = null)
    {
        if (positionOverride is not null)
        {
            var worldPos = GetWorldPos((Vector2)positionOverride);
            audioEvent.Start(worldPos);
        }
        else audioEvent.Start();
    }

    private Vector2 GetWorldPos(Vector2 pos) => OwnerEntity.Tilemap.Vector2ToWorldPoint(pos);
    
    protected internal override void OnPositionUpdate()
    {
        audioEvent.SetPosition(GetWorldPos(OwnerEntity.SmoothedPosition));
        base.OnPositionUpdate();
    }

    protected override void OnDestroy()
    {
        audioEvent.Dispose();
        base.OnDestroy();
    }
}