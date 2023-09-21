using Microsoft.Xna.Framework;

namespace MariEngine.Tiles;

public class TileBehavior
{
    public virtual void Update(GameTime gameTime)
    {
        
    }

    public virtual void OnPlaced()
    {
        
    }

    public virtual void OnMined()
    {
        
    }

    public virtual void OnSteppedOn(TileEntity steppingEntity)
    {
        
    }
}