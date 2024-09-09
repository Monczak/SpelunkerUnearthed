using System.Collections.Generic;
using MariEngine.Components;
using MariEngine.Loading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MariEngine.Rendering;

[Exclusive]
public abstract class Renderer([Inject] GraphicsDevice graphicsDevice, [Inject] Camera camera) : Component
{
    protected readonly Camera Camera = camera;
    protected readonly GraphicsDevice GraphicsDevice = graphicsDevice;

    protected readonly List<RendererEffect> Effects = [];

    public void AddEffect(RendererEffect effect)
    {
        Effects.Add(effect);
        Effects.Sort((effect1, effect2) => effect1.Priority - effect2.Priority);
    }

    [Special] public int Layer { get; set; } = 0;

    internal void DoRender(SpriteBatch spriteBatch)
    {
        if (Enabled)
            Render(spriteBatch);
    }

    protected abstract void Render(SpriteBatch spriteBatch);

    protected abstract Vector2 CalculateCenterOffset();
}