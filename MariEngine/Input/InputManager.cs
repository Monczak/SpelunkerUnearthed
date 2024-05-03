using System;
using System.Collections.Generic;
using System.Linq;
using MariEngine.Logging;
using MariEngine.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MariEngine.Input;

public class InputManager : Service
{
    private readonly Dictionary<string, InputEvent> inputEvents = new();

    private HashSet<Keys> pressedKeys = [];
    private HashSet<Keys> previousPressedKeys = [];

    private readonly Dictionary<InputEvent, HashSet<InputHandler>> pressedHandlers = new();
    private readonly Dictionary<InputEvent, HashSet<InputHandler>> releasedHandlers = new();

    private readonly List<PassThroughInputHandler> passThroughPressedHandlers = [];
    private readonly List<PassThroughInputHandler> passThroughReleasedHandlers = [];

    public delegate void InputHandler();
    public delegate void PassThroughInputHandler(Keys key);

    public void RegisterEvent(InputEvent inputEvent)
    {
        inputEvents[inputEvent.Name] = inputEvent;
        pressedHandlers[inputEvent] = [];
        releasedHandlers[inputEvent] = [];
    }

    public void BindKey(string inputName, Keys newKey)
    {
        inputEvents[inputName].Key = newKey;
    }

    public void OnPressed(string inputName, InputHandler handler)
    {
        pressedHandlers[inputEvents[inputName]].Add(handler);
    }
    
    public void OnReleased(string inputName, InputHandler handler)
    {
        releasedHandlers[inputEvents[inputName]].Add(handler);
    }

    public void OnPressedPassThrough(PassThroughInputHandler handler)
    {
        passThroughPressedHandlers.Add(handler);
    }
    
    public void OnReleasedPassThrough(PassThroughInputHandler handler)
    {
        passThroughReleasedHandlers.Add(handler);
    }

    public bool IsHeld(string inputName) => pressedKeys.Contains(inputEvents[inputName].Key);

    public void UnbindOnPressed(string inputName, InputHandler handler)
    {
        // Logger.Log($"Unbound pressed from {inputName}");
        pressedHandlers[inputEvents[inputName]].Remove(handler);
    }
    
    public void UnbindOnReleased(string inputName, InputHandler handler)
    {
        // Logger.Log($"Unbound released from {inputName}");
        releasedHandlers[inputEvents[inputName]].Remove(handler);
    }
    
    public void UnbindOnPressedPassThrough(PassThroughInputHandler handler)
    {
        passThroughPressedHandlers.Remove(handler);
    }
    
    public void UnbindOnReleasedPassThrough(PassThroughInputHandler handler)
    {
        passThroughReleasedHandlers.Remove(handler);
    }

    public override void Update(GameTime gameTime)
    {
        var state = Keyboard.GetState();
        pressedKeys = [..state.GetPressedKeys()];

        foreach (var key in pressedKeys.Except(previousPressedKeys))
        {
            foreach (var handler in passThroughPressedHandlers) handler(key);
        }
        
        foreach (var key in previousPressedKeys.Except(pressedKeys))
        {
            foreach (var handler in passThroughReleasedHandlers) handler(key);
        }

        foreach (InputEvent inputEvent in inputEvents.Values)
        {
            if (pressedKeys.Contains(inputEvent.Key) && !previousPressedKeys.Contains(inputEvent.Key))
            {
                foreach (InputHandler handler in pressedHandlers[inputEvent])
                    handler();
            }
            
            if (!pressedKeys.Contains(inputEvent.Key) && previousPressedKeys.Contains(inputEvent.Key))
            {
                foreach (InputHandler handler in releasedHandlers[inputEvent])
                    handler();
            }
        }

        previousPressedKeys = [..pressedKeys];
    }
}