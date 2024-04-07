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
    private readonly bool oneShot;
    private readonly bool global;
    
    private readonly List<EventInstance> instances = [];
    private int instanceIndex;
    private Vector2 position;
    private readonly Dictionary<string, (float, bool)> parameters = new();

    private const int OneShotInstanceLimit = 10;

    private Vector2 listenerPos;

    public bool Disposed { get; private set; } = false;

    public AudioEvent(EventDescription description, bool oneShot = false, bool global = false)
    {
        this.oneShot = oneShot;
        this.global = global;
        
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
    
    internal void SetListenerPosition(Vector2 pos)
    {
        listenerPos = pos;
        foreach (var instance in instances)
        {
            SetupEventPosition3D(instance, position);
        }
    }

    private EventInstance GetEvent()
    {
        var theEvent = instances[instanceIndex];
        instanceIndex = (instanceIndex + 1) % instances.Count; 

        foreach (var (name, (value, ignoreSeekSpeed)) in parameters)
            theEvent.SetParameterValue(name, value, ignoreSeekSpeed);
        
        return theEvent;
    }

    private void SetupEventPosition3D(EventInstance theEvent, Vector2 pos)
    {
        var relativePos = GetPositionRelativeToListener(pos);
        theEvent.Position3D = new Vector3(relativePos.X, relativePos.Y, 0);
    }

    public void Start() => GetEvent().Start();

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
        if (global) return Vector2.Zero;
        
        var delta = pos - listenerPos;
        return delta;
    }

    public void SetPosition(Vector2 pos)
    {
        position = pos;
    }

    public void SetPosition(Coord coord) => SetPosition((Vector2)coord);

    public void Dispose()
    {
        foreach (var instance in instances)
            instance.Dispose();
        Disposed = true;
    }
}