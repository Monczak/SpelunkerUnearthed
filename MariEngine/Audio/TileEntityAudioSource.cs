using System.Collections.Generic;
using MariEngine.Components;
using Microsoft.Xna.Framework;

namespace MariEngine.Audio;

public class TileEntityAudioSource(AudioEvent audioEvent) : TileEntityComponent
{
    private List<AudioParameterAutomation> automations = [];

    public void AddAutomation(AudioParameterAutomation automation)
    {
        automations.Add(automation);
    }
    
    public void Play(Coord? positionOverride = null)
    {
        foreach (var automation in automations) 
            automation.Apply(audioEvent);
        
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