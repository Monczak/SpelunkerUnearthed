using Microsoft.Xna.Framework.Graphics;
using SpelunkerUnearthed.Engine.Components;

namespace SpelunkerUnearthed.Engine.Rendering;

[Exclusive]
public abstract class Renderer : Component
{
    public abstract void Render(SpriteBatch spriteBatch);
}