using Microsoft.Xna.Framework;

namespace MariEngine.Audio;

public abstract class AudioParameterAutomation
{
     protected internal abstract void Apply(AudioEvent audioEvent);
}