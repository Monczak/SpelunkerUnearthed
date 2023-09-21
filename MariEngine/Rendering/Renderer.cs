using MariEngine.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MariEngine.Rendering;

[Exclusive]
public abstract class Renderer : Component
{
    protected Camera camera;

    protected Renderer(Camera camera)
    {
        this.camera = camera;
    }
    
    public abstract void Render(SpriteBatch spriteBatch);

    protected abstract Vector2 CalculateCenterOffset();
}