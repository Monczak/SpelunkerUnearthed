using Microsoft.Xna.Framework;

namespace MariEngine.Tiles;

public abstract class TileBehavior
{
    // TODO: Add Tile parameter
    public virtual void Update(GameTime gameTime)
    {
        
    }

    public virtual void OnPlaced(Tile tile, Coord position)
    {
        
    }

    public virtual void OnMined(Tile tile, Coord position)
    {
        
    }

    public virtual void OnSteppedOn(Tile tile, Coord position, TileEntity steppingEntity)
    {
        
    }
}