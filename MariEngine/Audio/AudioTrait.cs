using System;
using Microsoft.Xna.Framework;

namespace MariEngine.Audio;

public abstract class AudioTrait
{
     protected internal abstract void Apply(AudioEvent audioEvent);
}