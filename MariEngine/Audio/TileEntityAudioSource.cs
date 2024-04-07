using System.Collections.Generic;
using MariEngine.Components;
using Microsoft.Xna.Framework;

namespace MariEngine.Audio;

public class TileEntityAudioSource(PositionalAudioSource audioSource) : TileEntityComponent
{
    private Vector2 GetWorldPos(Vector2 pos) => OwnerEntity.Tilemap.Vector2ToWorldPoint(pos);

    public void Play(string eventId) => audioSource.Play(eventId);
    
    protected internal override void OnPositionUpdate()
    {
        audioSource.SetPosition(GetWorldPos(OwnerEntity.SmoothedPosition));
        base.OnPositionUpdate();
    }

    protected override void OnDestroy()
    {
        audioSource.Dispose();
        base.OnDestroy();
    }
}