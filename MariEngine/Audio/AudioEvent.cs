using System;
using System.Collections.Generic;
using FMOD;
using FmodForFoxes;
using MariEngine.Utils;
using Microsoft.Xna.Framework;
using FmodForFoxes.Studio;
using MariEngine.Logging;

namespace MariEngine.Audio;

// TODO: This needs some cleanup (converting Vector2 to Vector3)
public class AudioEvent : IDisposable
{
    private readonly IAudioListener listener;
    private readonly bool oneShot;
    
    private readonly List<EventInstance> instances = [];
    private int instanceIndex;
    private Vector2 position;
    private readonly Dictionary<string, (float, bool)> parameters = new();

    private const int OneShotInstanceLimit = 10;

    public AudioEvent(EventDescription description, IAudioListener listener, bool oneShot = false)
    {
        this.listener = listener;
        this.oneShot = oneShot;
        
        if (oneShot)
        {
            for (var i = 0; i < OneShotInstanceLimit; i++)
                instances.Add(description.CreateInstance());
        }
        else
        {
            instances.Add(description.CreateInstance());
        }
    }

    private EventInstance GetEvent(Vector2? positionOverride = null)
    {
        var theEvent = instances[instanceIndex];
        instanceIndex = (instanceIndex + 1) % instances.Count; 
        
        var relativePos = GetPositionRelativeToListener(positionOverride ?? position);
        theEvent.Position3D = new Vector3(relativePos.X, relativePos.Y, 0);
        
        foreach (var (name, (value, ignoreSeekSpeed)) in parameters)
            theEvent.SetParameterValue(name, value, ignoreSeekSpeed);
        
        return theEvent;
    }
    
    public void Start(Vector2? positionOverride = null) => GetEvent(positionOverride).Start();

    public void Stop()
    {
        foreach (var instance in instances)
            instance.Stop();
    }

    public void SetParameterValue(string name, float value, bool ignoreSeekSpeed = false)
    {
        parameters[name] = (value, ignoreSeekSpeed);
        
        if (!oneShot)
            instances[0].SetParameterValue(name, value, ignoreSeekSpeed);
    }

    private Vector2 GetPositionRelativeToListener(Vector2 pos)
    {
        if (listener is null) return Vector2.Zero;
        
        var listenerPos = listener.GetPosition();
        var delta = pos - listenerPos;
        return delta;
    }

    public void SetPosition(Vector2 pos)
    {
        position = pos;
        
        if (!oneShot)
        {
            var relativePos = GetPositionRelativeToListener(position);
            instances[0].Position3D = new Vector3(relativePos.X, relativePos.Y, 0);
        }
    }

    public void SetPosition(Coord coord) => SetPosition((Vector2)coord);

    public void Dispose()
    {
        foreach (var instance in instances)
            instance.Dispose();
    }
}