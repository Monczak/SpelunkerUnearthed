using MariEngine.Components;
using MariEngine.Light;
using Microsoft.Xna.Framework.Graphics;

namespace MariEngine.Rendering;

public abstract class TileEntityRenderer : TileEntityComponent
{
    public abstract void Render(SpriteBatch spriteBatch, Camera camera, GraphicsDevice graphicsDevice, LightMap lightMap);

    public abstract Bounds GetCullingBounds();
}