using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace MariEngine.Audio;

public class PositionalAudioSource : IDisposable
{
    private readonly Dictionary<string, AudioEvent> events = new();
    private readonly List<AudioTrait> automations = [];

    public void SetAutomationPositionProvider(Func<Vector2> provider)
    {
        foreach (var automation in automations)
            automation.SetPositionProvider(provider);
    }
    
    public PositionalAudioSource WithEvent(string eventId, AudioEvent audioEvent)
    {
        events.Add(eventId, audioEvent);
        return this;
    }
    
    public PositionalAudioSource WithAutomation(AudioTrait automation)
    {
        automations.Add(automation);
        return this;
    }
    
    public void Play(string eventId, Vector2? position = null)
    {
        var audioEvent = events[eventId];
        
        foreach (var automation in automations) 
            automation.Apply(audioEvent);
        
        if (position is not null)
            audioEvent.SetPosition(position.Value);
        
        audioEvent.Start();
    }

    public void SetPosition(Vector2 position)
    {
        foreach (var audioEvent in events.Values)
            audioEvent.SetPosition(position);
    }

    public void Dispose()
    {
        foreach (var audioEvent in events.Values)
            audioEvent.Dispose();
    }
}