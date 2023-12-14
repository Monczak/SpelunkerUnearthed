using System.Collections.Generic;
using FmodForFoxes.Studio;
using MariEngine.Components;
using MariEngine.Services;
using SpelunkerUnearthed.Scripts.Managers;
using SpelunkerUnearthed.Scripts.MapGeneration.Biomes;

namespace SpelunkerUnearthed.Scripts.Components;

public class AmbienceController : Component
{
    private readonly EventInstance ambienceEvent = ServiceRegistry.Get<AudioManager>().GetEvent("event:/Ambience");

    public void Play()
    {
        ambienceEvent.Start();
    }

    public void SetBiomeAmbience(Biome biome)
    {
        ambienceEvent.SetParameterValue("Biome", biome.BiomeAmbience);
    }
    
    protected override void OnDestroy()
    {
        ambienceEvent.Dispose();
    }
}