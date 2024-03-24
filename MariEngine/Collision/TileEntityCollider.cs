using MariEngine.Components;

namespace MariEngine.Collision;

public abstract class TileEntityCollider : TileEntityComponent
{
    public abstract CoordBounds BoundingBox { get; }

    public abstract CollisionGroup? GetCollisionGroup(Coord localPos);
}