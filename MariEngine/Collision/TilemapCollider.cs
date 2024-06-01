using System.Linq;
using MariEngine.Tiles;
using MariEngine.Utils;

namespace MariEngine.Collision;

public class TilemapCollider(Coord? spatialPartitionCellSize = null) : Collider
{
    private Tilemap tilemap;
    private SpatialPartition<TileEntityCollider> spatialPartition;

    private readonly Coord cellSize = spatialPartitionCellSize ?? Coord.One * 8;

    protected internal override void Initialize()
    {
        spatialPartition = new SpatialPartition<TileEntityCollider>(cellSize);
        tilemap = GetComponent<Tilemap>();
        tilemap.TileEntityAdded += OnTileEntityAdded;
        tilemap.TileEntityRemoved += OnTileEntityRemoved;

        foreach (var entity in tilemap.TileEntities)
        {
            OnTileEntityAdded(entity);
        }
    }
    
    private void OnTileEntityAdded(TileEntity tileEntity)
    {
        if (spatialPartitionCellSize is null) return;
        
        var collider = tileEntity.GetComponent<TileEntityCollider>();
        if (collider is null) return;
        
        spatialPartition.Add(collider, collider.OwnerEntity.Position);
        
        tileEntity.PositionUpdated += OnTileEntityPositionUpdated;
    }

    private void OnTileEntityPositionUpdated(TileEntity sender, Coord oldPos, Coord newPos)
    {
        if (spatialPartitionCellSize is null) return;
        
        if (oldPos == newPos)
            return;
        
        spatialPartition.UpdatePosition(sender.GetComponent<TileEntityCollider>(), oldPos, newPos);
    }

    private void OnTileEntityRemoved(TileEntity tileEntity)
    {
        if (spatialPartitionCellSize is null) return;
        
        var collider = tileEntity.GetComponent<TileEntityCollider>();
        if (collider is null) return;

        spatialPartition.Remove(collider, collider.OwnerEntity.Position);

        tileEntity.PositionUpdated -= OnTileEntityPositionUpdated;
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

        var collides = false;
        foreach (var worldPos in colliderBounds.Coords)
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

        var collidersToCheck = spatialPartitionCellSize is null
            ? tilemap.TileEntities.Select(t => t.GetComponent<TileEntityCollider>())
            : spatialPartition.Check(collider, coord);
        
        foreach (var otherCollider in collidersToCheck)
        {
            if (otherCollider == collider) continue;
            if (otherCollider is null) continue;

            var otherColliderBounds = GetColliderBoundsAtPos(otherCollider, otherCollider.OwnerEntity.Position);
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