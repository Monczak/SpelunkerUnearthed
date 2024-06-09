using MariEngine.Light;
using MariEngine.Loading;
using MariEngine.Services;
using MariEngine.Sprites;
using MariEngine.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MariEngine.Rendering;

public class TileEntitySpriteRenderer([InjectResource] Sprite sprite) : TileEntityRenderer
{
    public Sprite Sprite => sprite;
    
    public override void Render(SpriteBatch spriteBatch, Camera camera, GraphicsDevice graphicsDevice, LightMap lightMap)
    {
        if (sprite is null) return;
        
        foreach (Coord coord in sprite.Tiles.Coords)
        {
            var tint = lightMap.GetRenderedLight(OwnerEntity.Position + coord - GetSpriteOffset());
            var tile = sprite.Tiles[coord];
            if (tile is null) continue;
            
            ServiceRegistry.Get<TileAtlas>().DrawTile(
                spriteBatch,
                OwnerEntity.Tilemap.Vector2ToWorldPoint(OwnerEntity.SmoothedPosition + (Vector2)coord - (Vector2)GetSpriteOffset()) * camera.TileSize,
                tile.Id,
                tint
            );
        }
    }

    private Coord GetSpriteOffset() => (sprite.Size - Coord.One) / 2;

    public override Bounds GetCullingBounds() => new(OwnerEntity.SmoothedPosition - (Vector2)GetSpriteOffset(), (Vector2)sprite.Size);
}