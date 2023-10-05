using MariEngine.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MariEngine.Rendering;

[Exclusive]
public abstract class Renderer : Component
{
    protected Camera camera;
    protected GraphicsDevice graphicsDevice;

    public int Layer { get; set; } = 0;

    protected Renderer(GraphicsDevice graphicsDevice, Camera camera)
    {
        this.graphicsDevice = graphicsDevice;
        this.camera = camera;
    }

    internal void DoRender(SpriteBatch spriteBatch)
    {
        if (Enabled)
            Render(spriteBatch);
    }

    protected abstract void Render(SpriteBatch spriteBatch);

    protected abstract Vector2 CalculateCenterOffset();
}