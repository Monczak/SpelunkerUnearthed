namespace MariEngine.Collision;

public class BasicTileEntityCollider(CoordBounds boundingBox, CollisionGroup collisionGroup) : TileEntityCollider
{
    public override CoordBounds BoundingBox => boundingBox;
    public override CollisionGroup? GetCollisionGroup(Coord localPos) => collisionGroup;
}