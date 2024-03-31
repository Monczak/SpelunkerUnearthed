using System.Collections.Generic;
using FmodForFoxes.Studio;
using MariEngine.Audio;
using MariEngine.Components;
using MariEngine.Services;
using Microsoft.Xna.Framework;
using SpelunkerUnearthed.Scripts.Managers;
using SpelunkerUnearthed.Scripts.MapGeneration.Biomes;

namespace SpelunkerUnearthed.Scripts.Components;

public class AmbienceController : Component
{
    private readonly AudioEvent ambienceEvent = ServiceRegistry.Get<AudioManager>().GetEvent("event:/Ambience");

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
        ambienceEvent.Stop();
        ambienceEvent.Dispose();
    }
}