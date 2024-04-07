using System;
using System.Collections.Generic;
using FMOD;
using FmodForFoxes;
using MariEngine.Utils;
using Microsoft.Xna.Framework;
using FmodForFoxes.Studio;

namespace MariEngine.Audio;

// TODO: This needs some cleanup (converting Vector2 to Vector3)
public class AudioEvent(EventDescription description, IAudioListener listener, bool oneShot = false) : IDisposable
{
    private readonly EventInstance fmodEvent = oneShot ? null : description.CreateInstance();
    private Vector2 position;
    private readonly Dictionary<string, (float, bool)> parameters = new();

    private EventInstance GetEvent(Vector2? positionOverride = null)
    {
        // TODO: Cleanup one-shot events (they may not be disposed properly)
        var theEvent = oneShot ? description.CreateInstance() : fmodEvent;
        var relativePos = GetPositionRelativeToListener(positionOverride ?? position);

        theEvent.Position3D = new Vector3(relativePos.X, relativePos.Y, 0);
        
        foreach (var (name, (value, ignoreSeekSpeed)) in parameters)
            theEvent.SetParameterValue(name, value, ignoreSeekSpeed);
        
        return theEvent;
    }
    
    public void Start(Vector2? positionOverride = null) => GetEvent(positionOverride).Start();
    public void Stop() => fmodEvent?.Stop();

    public void SetParameterValue(string name, float value, bool ignoreSeekSpeed = false)
    {
        parameters[name] = (value, ignoreSeekSpeed);
        fmodEvent?.SetParameterValue(name, value, ignoreSeekSpeed);
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

        if (fmodEvent is not null)
        {
            var relativePos = GetPositionRelativeToListener(pos);
            fmodEvent.Position3D = new Vector3(relativePos.X, relativePos.Y, 0);
        }
    }

    public void SetPosition(Coord coord) => SetPosition((Vector2)coord);

    public void Dispose()
    {
        fmodEvent?.Dispose();
    }
}