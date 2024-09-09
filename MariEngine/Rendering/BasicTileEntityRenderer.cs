using System.Collections.Generic;
using MariEngine.Light;
using MariEngine.Services;
using MariEngine.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MariEngine.Rendering;

public class BasicTileEntityRenderer(Tile tile) : TileEntityRenderer
{
    public override void Render(SpriteBatch spriteBatch, Camera camera, GraphicsDevice graphicsDevice,
        IList<TilemapRendererEffect> effects, GameTime gameTime)
    {
        if (tile is null) return;
        
        var foregroundColor = tile.ForegroundColor;
        var backgroundColor = tile.BackgroundColor;

        foreach (var effect in effects)
        {
            foregroundColor = effect.Apply(foregroundColor, OwnerEntity.Position, gameTime);
            backgroundColor = effect.Apply(backgroundColor, OwnerEntity.Position, gameTime);
        }
            
        ServiceRegistry.Get<TileAtlas>().DrawTile(
            spriteBatch,
            OwnerEntity.Tilemap.Vector2ToWorldPoint(OwnerEntity.SmoothedPosition) * camera.TileSize,
            tile.Id,
            foregroundColor,
            backgroundColor
        );
    }

    public override Bounds GetCullingBounds() => new(OwnerEntity.SmoothedPosition, Vector2.One);
}