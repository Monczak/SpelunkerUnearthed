using MariEngine.Tiles;
using Microsoft.Xna.Framework;

namespace MariEngine.Rendering;

public abstract class TilemapRendererEffect
{
    public virtual TilemapLayer LayerMask { get; init; } = TilemapLayer.All;
    public virtual bool ApplyToTileEntities { get; init; } = true;
    
    public virtual int Priority { get; init; }
    
    public abstract Color Apply(Color input, Coord worldPos, GameTime gameTime);
}