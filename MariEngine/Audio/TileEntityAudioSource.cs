using System.Collections.Generic;
using MariEngine.Components;
using Microsoft.Xna.Framework;

namespace MariEngine.Audio;

public class TileEntityAudioSource : TileEntityComponent
{
    // TODO: Refactor this out to a separate PositionalAudioSource?
    private readonly Dictionary<string, AudioEvent> events = new();
    private readonly List<AudioParameterAutomation> automations = [];

    public TileEntityAudioSource WithEvent(string eventId, AudioEvent audioEvent)
    {
        events.Add(eventId, audioEvent);
        return this;
    }
    
    public void AddAutomation(AudioParameterAutomation automation)
    {
        automations.Add(automation);
    }
    
    public void Play(string eventId, Coord? positionOverride = null)
    {
        var audioEvent = events[eventId];
        
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
        foreach (var audioEvent in events.Values)
            audioEvent.SetPosition(GetWorldPos(OwnerEntity.SmoothedPosition));
        
        base.OnPositionUpdate();
    }

    protected override void OnDestroy()
    {
        foreach (var audioEvent in events.Values)
            audioEvent.Dispose();
        
        base.OnDestroy();
    }
}