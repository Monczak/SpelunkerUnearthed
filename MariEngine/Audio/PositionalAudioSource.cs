using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace MariEngine.Audio;

public class PositionalAudioSource : IDisposable
{
    private readonly Dictionary<string, AudioEvent> events = new();
    private readonly List<AudioParameterAutomation> automations = [];

    public PositionalAudioSource WithEvent(string eventId, AudioEvent audioEvent)
    {
        events.Add(eventId, audioEvent);
        return this;
    }
    
    public PositionalAudioSource WithAutomation(AudioParameterAutomation automation)
    {
        automations.Add(automation);
        return this;
    }
    
    public void Play(string eventId)
    {
        var audioEvent = events[eventId];
        
        foreach (var automation in automations) 
            automation.Apply(audioEvent);
        
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