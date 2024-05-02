using MariEngine.Components;
using MariEngine.Utils;

namespace MariEngine.Collision;

public abstract class TileEntityCollider : TileEntityComponent, ICoordBoundsProvider
{
    public abstract CoordBounds BoundingBox { get; }

    public abstract CollisionGroup? GetCollisionGroup(Coord localPos);
    
    public CoordBounds GetBounds() => BoundingBox;
}