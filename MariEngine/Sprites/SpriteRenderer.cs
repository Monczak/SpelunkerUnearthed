using MariEngine.Light;
using MariEngine.Rendering;
using MariEngine.Tiles;
using Microsoft.Xna.Framework.Graphics;

namespace MariEngine.Sprites;

public class SpriteRenderer : TileEntityRenderer
{
    public override void Render(SpriteBatch spriteBatch, Camera camera, GraphicsDevice graphicsDevice, LightMap lightMap)
    {
        throw new System.NotImplementedException();
    }

    public override Bounds GetCullingBounds()
    {
        throw new System.NotImplementedException();
    }
}