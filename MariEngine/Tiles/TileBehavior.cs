using Microsoft.Xna.Framework;

namespace MariEngine.Tiles;

public abstract class TileBehavior
{
    public virtual void Update(GameTime gameTime)
    {
        
    }

    public virtual void OnPlaced(Coord position)
    {
        
    }

    public virtual void OnMined(Coord position)
    {
        
    }

    public virtual void OnSteppedOn(Coord position, TileEntity steppingEntity)
    {
        
    }
}