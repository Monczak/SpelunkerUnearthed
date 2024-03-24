using MariEngine.Tiles;

namespace MariEngine.Collision;

public class TilemapCollider : Collider
{
    private Tilemap tilemap;

    protected override void OnAttach()
    {
        tilemap = GetComponent<Tilemap>();
    }

    public override bool TileEntityCollides(TileEntity tileEntity, Coord coord)
    {
        var collider = tileEntity.GetComponent<TileEntityCollider>();
        if (collider is null) return false;

        return ResolveTilemapCollision(collider, coord) || ResolveTileEntityCollision(collider, coord);
    }
    
    private CoordBounds GetColliderBoundsAtPos(TileEntityCollider collider, Coord coord) => new(coord + collider.BoundingBox.TopLeft, collider.BoundingBox.Size);

    private bool ResolveTilemapCollision(TileEntityCollider collider, Coord coord)
    {
        if (!tilemap.IsInBounds(coord)) return true;
        var colliderBounds = GetColliderBoundsAtPos(collider, coord);

        bool collides = false;
        foreach (Coord worldPos in colliderBounds.Coords)
        {
            var tilemapTileColGroup = tilemap.Get(worldPos, Tilemap.BaseLayer).CollisionGroup; // TODO: Handle collision on other layers?
            var tileEntityColGroup = collider.GetCollisionGroup(worldPos - colliderBounds.TopLeft);
            if (tileEntityColGroup is null) continue;
            
            collides |= (tileEntityColGroup & tilemapTileColGroup) != 0;
        }
        return collides;
    }

    private bool ResolveTileEntityCollision(TileEntityCollider collider, Coord coord)
    {
        var collides = false;
        var colliderBounds = GetColliderBoundsAtPos(collider, coord);
        foreach (var entity in tilemap.TileEntities) // TODO: Use spatial partitioning to avoid checking tile entities far away
        {
            if (entity == collider.OwnerEntity) continue;
            
            var otherCollider = entity.GetComponent<TileEntityCollider>();
            if (otherCollider is null) continue;

            var otherColliderBounds = GetColliderBoundsAtPos(otherCollider, entity.Position);
            var overlap = CoordBounds.GetOverlap(colliderBounds, otherColliderBounds);
            if (overlap is null) continue;

            foreach (var pos in overlap.Value.Coords)
            {
                var localPos = pos - colliderBounds.TopLeft;
                var otherLocalPos = pos - otherColliderBounds.TopLeft;

                var collisionGroup = collider.GetCollisionGroup(localPos);
                var otherCollisionGroup = otherCollider.GetCollisionGroup(otherLocalPos);
                if (collisionGroup is null || otherCollisionGroup is null) continue;
                
                collides |= (collisionGroup & otherCollisionGroup) != 0;
            }
        }

        return collides;
    }
}