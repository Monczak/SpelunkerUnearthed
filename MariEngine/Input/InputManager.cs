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

    private readonly Dictionary<object, Dictionary<InputEvent, HashSet<InputHandler>>> pressedHandlers = new();
    private readonly Dictionary<object, Dictionary<InputEvent, HashSet<InputHandler>>> releasedHandlers = new();

    private readonly Dictionary<object, List<PassThroughInputHandler>> passThroughPressedHandlers = [];
    private readonly Dictionary<object, List<PassThroughInputHandler>> passThroughReleasedHandlers = [];

    public delegate void InputHandler();
    public delegate void PassThroughInputHandler(Keys key);

    public class InputManagerContext(InputManager manager, object context) : IDisposable
    {
        public void OnPressed(string inputName, InputHandler handler) => manager.OnPressed(context, inputName, handler);
        public void OnReleased(string inputName, InputHandler handler) => manager.OnReleased(context, inputName, handler);

        public void OnPressedPassThrough(PassThroughInputHandler handler) =>
            manager.OnPressedPassThrough(context, handler);
        public void OnReleasedPassThrough(PassThroughInputHandler handler) =>
            manager.OnReleasedPassThrough(context, handler);

        public void UnbindOnPressed(string inputName, InputHandler handler) =>
            manager.UnbindOnPressed(context, inputName, handler);
        public void UnbindOnReleased(string inputName, InputHandler handler) =>
            manager.UnbindOnReleased(context, inputName, handler);

        public void UnbindOnPressedPassThrough(PassThroughInputHandler handler) =>
            manager.UnbindOnPressedPassThrough(context, handler);
        public void UnbindOnReleasedPassThrough(PassThroughInputHandler handler) =>
            manager.UnbindOnReleasedPassThrough(context, handler);

        public void Dispose()
        {
        }
    }

    public InputManagerContext CreateContext(object context) => new(this, context);
    

    public void RegisterEvent(InputEvent inputEvent)
    {
        inputEvents[inputEvent.Name] = inputEvent;
    }

    public void BindKey(string inputName, Keys newKey)
    {
        inputEvents[inputName].Key = newKey;
    }

    public void OnPressed(object context, string inputName, InputHandler handler)
    {
        pressedHandlers.TryAdd(context, new Dictionary<InputEvent, HashSet<InputHandler>>());
        pressedHandlers[context].TryAdd(inputEvents[inputName], []);
        pressedHandlers[context][inputEvents[inputName]].Add(handler);
    }
    
    public void OnReleased(object context, string inputName, InputHandler handler)
    {
        releasedHandlers.TryAdd(context, new Dictionary<InputEvent, HashSet<InputHandler>>());
        releasedHandlers[context].TryAdd(inputEvents[inputName], []);
        releasedHandlers[context][inputEvents[inputName]].Add(handler);
    }

    public void OnPressedPassThrough(object context, PassThroughInputHandler handler)
    {
        passThroughPressedHandlers.TryAdd(context, []);
        passThroughPressedHandlers[context].Add(handler);
    }
    
    public void OnReleasedPassThrough(object context, PassThroughInputHandler handler)
    {
        passThroughReleasedHandlers.TryAdd(context, []);
        passThroughReleasedHandlers[context].Add(handler);
    }

    public bool IsHeld(string inputName) => pressedKeys.Contains(inputEvents[inputName].Key);

    public void UnbindOnPressed(object context, string inputName, InputHandler handler)
    {
        // Logger.Log($"Unbound pressed from {inputName}");
        pressedHandlers[context][inputEvents[inputName]].Remove(handler);
    }
    
    public void UnbindOnReleased(object context, string inputName, InputHandler handler)
    {
        // Logger.Log($"Unbound released from {inputName}");
        releasedHandlers[context][inputEvents[inputName]].Remove(handler);
    }
    
    public void UnbindOnPressedPassThrough(object context, PassThroughInputHandler handler)
    {
        passThroughPressedHandlers[context].Remove(handler);
    }
    
    public void UnbindOnReleasedPassThrough(object context, PassThroughInputHandler handler)
    {
        passThroughReleasedHandlers[context].Remove(handler);
    }

    public void UnbindAllNormal(object context)
    {
        pressedHandlers.Remove(context);
        releasedHandlers.Remove(context);
    }

    public void UnbindAllPassThrough(object context)
    {
        passThroughPressedHandlers.Remove(context);
        passThroughReleasedHandlers.Remove(context);
    }

    public void UnbindAll(object context)
    {
        UnbindAllNormal(context);
        UnbindAllPassThrough(context);
    }

    public override void Update(GameTime gameTime)
    {
        var state = Keyboard.GetState();
        pressedKeys = [..state.GetPressedKeys()];

        foreach (var key in pressedKeys.Except(previousPressedKeys))
        {
            foreach (var (context, handlers) in passThroughPressedHandlers)
            {
                foreach (var handler in handlers) handler(key);
            }
        }
        
        foreach (var key in previousPressedKeys.Except(pressedKeys))
        {
            foreach (var (context, handlers) in passThroughReleasedHandlers)
            {
                foreach (var handler in handlers) handler(key);
            }
            
        }

        foreach (InputEvent inputEvent in inputEvents.Values)
        {
            if (pressedKeys.Contains(inputEvent.Key) && !previousPressedKeys.Contains(inputEvent.Key))
            {
                foreach (var (context, handlers) in pressedHandlers.ToList())
                {
                    if (!handlers.ContainsKey(inputEvent)) continue;
                    
                    foreach (var handler in handlers[inputEvent])
                        handler();
                }
            }
            
            if (!pressedKeys.Contains(inputEvent.Key) && previousPressedKeys.Contains(inputEvent.Key))
            {
                foreach (var (context, handlers) in releasedHandlers.ToList())
                {
                    if (!handlers.ContainsKey(inputEvent)) continue;
                    
                    foreach (var handler in handlers[inputEvent])
                        handler();
                }
            }
        }

        previousPressedKeys = [..pressedKeys];
    }
}