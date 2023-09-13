using SpelunkerUnearthed.Engine.Components;
using SpelunkerUnearthed.Engine.Tiles;

namespace SpelunkerUnearthed.Engine.Collision;

public abstract class Collider : Component
{
    public virtual bool TileCollides(Tile tile, Coord coord) => false;
    public virtual bool TileEntityCollides(TileEntity tileEntity, Coord coord) => false;
    public virtual bool EntityCollides(Entity entity) => false;

    public bool TileEntityCollides(TileEntity tileEntity) => TileEntityCollides(tileEntity, tileEntity.Position);
    
    protected static bool DoTilesCollide(Tile tile1, Tile tile2)
    {
        return (tile1.CollisionGroup & tile2.CollisionGroup) != CollisionGroup.None;
    }
}