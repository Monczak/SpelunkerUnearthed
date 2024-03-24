using MariEngine.Light;
using MariEngine.Services;
using MariEngine.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MariEngine.Rendering;

public class BasicTileEntityRenderer(Tile tile) : TileEntityRenderer
{
    public override void Render(SpriteBatch spriteBatch, Camera camera, GraphicsDevice graphicsDevice, LightMap lightMap)
    {
        var tint = lightMap.GetRenderedLight(OwnerEntity.Position);

        if (tile is not null)
        {
            ServiceRegistry.Get<TileAtlas>().DrawTile(
                spriteBatch,
                OwnerEntity.Tilemap.Vector2ToWorldPoint(OwnerEntity.SmoothedPosition) * camera.TileSize,
                tile.Id,
                tint
            );
        }
    }

    public override Bounds GetCullingBounds() => new(OwnerEntity.SmoothedPosition, Vector2.One);
}