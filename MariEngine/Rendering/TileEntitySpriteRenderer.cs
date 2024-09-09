using System.Collections.Generic;
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
    
    public override void Render(SpriteBatch spriteBatch, Camera camera, GraphicsDevice graphicsDevice,
        IList<TilemapRendererEffect> effects, GameTime gameTime)
    {
        if (sprite is null) return;
        
        foreach (Coord coord in sprite.Tiles.Coords)
        {
            var tile = sprite.Tiles[coord];
            if (tile is null) continue;
            
            var foregroundColor = tile.ForegroundColor;
            var backgroundColor = tile.BackgroundColor;

            var tilePos = OwnerEntity.Position + coord - GetSpriteOffset();
            
            foreach (var effect in effects)
            {
                if (!effect.ApplyToTileEntities) continue;
                
                foregroundColor = effect.Apply(foregroundColor, tilePos, gameTime);
                backgroundColor = effect.Apply(backgroundColor, tilePos, gameTime);
            }
            
            ServiceRegistry.Get<TileAtlas>().DrawTile(
                spriteBatch,
                OwnerEntity.Tilemap.Vector2ToWorldPoint(OwnerEntity.SmoothedPosition + (Vector2)coord - (Vector2)GetSpriteOffset()) * camera.TileSize,
                tile.Id,
                foregroundColor,
                backgroundColor
            );
        }
    }

    private Coord GetSpriteOffset() => (sprite.Size - Coord.One) / 2;

    public override Bounds GetCullingBounds() => new(OwnerEntity.SmoothedPosition - (Vector2)GetSpriteOffset(), (Vector2)sprite.Size);
}