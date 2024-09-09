using System;
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

    [Special] public int Layer { get; set; } = 0;

    internal void DoRender(SpriteBatch spriteBatch, GameTime gameTime)
    {
        if (Enabled)
            Render(spriteBatch, gameTime);
    }

    protected abstract void Render(SpriteBatch spriteBatch, GameTime gameTime);

    protected abstract Vector2 CalculateCenterOffset();
}