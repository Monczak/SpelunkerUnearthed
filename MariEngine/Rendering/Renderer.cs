﻿using MariEngine.Components;
using MariEngine.Loading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MariEngine.Rendering;

[Exclusive]
public abstract class Renderer(GraphicsDevice graphicsDevice, Camera camera) : Component
{
    protected readonly Camera Camera = camera;
    protected readonly GraphicsDevice GraphicsDevice = graphicsDevice;

    public int Layer { get; set; } = 0;

    internal void DoRender(SpriteBatch spriteBatch)
    {
        if (Enabled)
            Render(spriteBatch);
    }

    protected abstract void Render(SpriteBatch spriteBatch);

    protected abstract Vector2 CalculateCenterOffset();
}