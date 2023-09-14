using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SpelunkerUnearthed.Engine.Logging;
using SpelunkerUnearthed.Engine.Services;

namespace SpelunkerUnearthed.Engine.Input;

public class InputManager : Service
{
    private Dictionary<string, InputEvent> inputEvents = new();

    private HashSet<Keys> pressedKeys = new();
    private HashSet<Keys> previousPressedKeys = new();

    private Dictionary<InputEvent, HashSet<InputHandler>> pressedHandlers = new();
    private Dictionary<InputEvent, HashSet<InputHandler>> releasedHandlers = new();

    public delegate void InputHandler();

    public void RegisterEvent(InputEvent inputEvent)
    {
        inputEvents[inputEvent.Name] = inputEvent;
        pressedHandlers[inputEvent] = new HashSet<InputHandler>();
        releasedHandlers[inputEvent] = new HashSet<InputHandler>();
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

    public override void Update()
    {
        var state = Keyboard.GetState();
        pressedKeys = new HashSet<Keys>(state.GetPressedKeys());

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

        previousPressedKeys = new HashSet<Keys>(pressedKeys);
    }
}