using System.Collections.Generic;
using System.Linq;
using FmodForFoxes.Studio;
using MariEngine.Logging;
using MariEngine.Services;

namespace MariEngine.Audio;

public class AudioManager : Service
{
    private readonly Dictionary<object, Dictionary<string, Bank>> banks = new();

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

    public EventInstance GetEvent(string path)
    {
        var eventDescription = StudioSystem.GetEvent(path);
        eventDescription.LoadSampleData();
        return eventDescription.CreateInstance();
    }
}