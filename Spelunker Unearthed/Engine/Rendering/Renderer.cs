using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpelunkerUnearthed.Engine.Components;

namespace SpelunkerUnearthed.Engine.Rendering;

[Exclusive]
public abstract class Renderer : Component
{
    public abstract void Render(SpriteBatch spriteBatch, Camera camera);

    protected abstract Vector2 CalculateCenterOffset(Camera camera);
}