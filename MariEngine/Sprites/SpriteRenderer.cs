using MariEngine.Light;
using MariEngine.Rendering;
using MariEngine.Services;
using MariEngine.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MariEngine.Sprites;

public class SpriteRenderer(Sprite sprite) : TileEntityRenderer
{
    public override void Render(SpriteBatch spriteBatch, Camera camera, GraphicsDevice graphicsDevice, LightMap lightMap)
    {
        var tint = lightMap.GetRenderedLight(OwnerEntity.Position);

        if (sprite is not null)
        {
            foreach (Coord coord in sprite.Tiles.Coords)
            {
                var tile = sprite.Tiles[coord];
                ServiceRegistry.Get<TileAtlas>().DrawTile(
                    spriteBatch,
                    OwnerEntity.Tilemap.Vector2ToWorldPoint(OwnerEntity.SmoothedPosition + (Vector2)coord - GetSpriteOffset()) * camera.TileSize,
                    tile.Id,
                    tint
                );
            }
            
        }
    }

    private Vector2 GetSpriteOffset() => (Vector2)(sprite.Size - Coord.One) / 2;

    public override Bounds GetCullingBounds() => new(OwnerEntity.SmoothedPosition - GetSpriteOffset(), (Vector2)sprite.Size);
}