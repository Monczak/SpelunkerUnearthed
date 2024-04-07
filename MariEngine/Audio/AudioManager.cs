using System.Collections.Generic;
using System.Linq;
using FmodForFoxes.Studio;
using MariEngine.Logging;
using MariEngine.Services;
using MariEngine.Utils;
using Microsoft.Xna.Framework;

namespace MariEngine.Audio;

public class AudioManager : Service
{
    private readonly Dictionary<object, Dictionary<string, Bank>> banks = new();

    private readonly HashSet<AudioEvent> activeEvents = [];
    private const float ListenerPositionSmoothing = 10f;

    public int ActiveEventCount => activeEvents.Count;

    public IAudioListener Listener { get; private set; }
    private Vector2 smoothedListenerPos;

    public void SetListener(IAudioListener listener) => Listener = listener;

    public Bank LoadBank(object context, string name)
    {
        if (banks.SelectMany(pair => pair.Value).Any(bankWithName => bankWithName.Key == name))
        {
            Logger.LogWarning($"Refusing to load audio bank {name} because it's already loaded");
            return null;
        }
        
        var bank = StudioSystem.LoadBank($"{name}.bank");

        if (!banks.ContainsKey(context))
            banks[context] = new Dictionary<string, Bank>();

        banks[context][name] = bank;
        Logger.Log($"Loaded audio bank {name}");

        return bank;
    }

    public void UnloadBank(object context, string name)
    {
        if (banks.TryGetValue(context, out var bankDict))
        {
            if (bankDict.TryGetValue(name, out var bank))
            {
                bank.Unload();
                bankDict.Remove(name);
                
                Logger.Log($"Unloaded audio bank {name}");
            }
        }
    }

    public void UnloadAllBanks(object context)
    {
        if (banks.TryGetValue(context, out var bankDict))
        {
            foreach (var bank in bankDict.Keys)
            {
                UnloadBank(context, bank);
            }

            banks.Remove(context);
        }
    }

    public AudioEvent GetEvent(string path, bool oneShot = false, bool global = false)
    {
        var eventDescription = StudioSystem.GetEvent(path);
        eventDescription.LoadSampleData();
        
        var audioEvent = new AudioEvent(eventDescription, oneShot, global);
        activeEvents.Add(audioEvent);
        return audioEvent;
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        smoothedListenerPos = Vector2.Lerp(smoothedListenerPos, Listener.GetPosition(),
            (float)(ListenerPositionSmoothing * gameTime.ElapsedGameTime.TotalSeconds));
        
        foreach (var audioEvent in activeEvents)
        {
            audioEvent.SetListenerPosition(smoothedListenerPos);
        }

        activeEvents.RemoveWhere(@event => @event.Disposed);
    }
}