using FmodForFoxes.Studio;
using MariEngine.Components;
using MariEngine.Services;
using SpelunkerUnearthed.Scripts.Managers;

namespace SpelunkerUnearthed.Scripts.Components;

public class AmbienceController : Component
{
    private readonly EventInstance ambienceEvent = ServiceRegistry.Get<AudioManager>().GetEvent("/Ambience");

    public void Play()
    {
        ambienceEvent.Start();
        ambienceEvent.SetParameterValue("Biome", 1);
    }

    protected override void OnDestroy()
    {
        ambienceEvent.Dispose();
    }
}