using System;
using Microsoft.Xna.Framework;

namespace MariEngine.Audio;

public abstract class AudioTrait
{
     protected internal abstract void Apply(AudioEvent audioEvent);

     private Func<Vector2> positionProvider;
     protected Vector2 GetPosition() => positionProvider();

     public AudioTrait WithPositionProvider(Func<Vector2> provider)
     {
          SetPositionProvider(provider);
          return this;
     }

     public void SetPositionProvider(Func<Vector2> provider) => positionProvider = provider;
}