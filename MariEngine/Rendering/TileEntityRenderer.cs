using System.Collections.Generic;
using MariEngine.Components;
using MariEngine.Light;
using Microsoft.Xna.Framework.Graphics;

namespace MariEngine.Rendering;

public abstract class TileEntityRenderer : TileEntityComponent
{
    public abstract void Render(SpriteBatch spriteBatch, Camera camera, GraphicsDevice graphicsDevice,
        IList<RendererEffect> effects);

    public abstract Bounds GetCullingBounds();
}