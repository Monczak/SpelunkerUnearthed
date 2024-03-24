using System.Collections.Generic;
using System.Linq;
using MariEngine.Tiles;

namespace MariEngine.Collision;

public class TilemapCollider(Coord? spatialPartitionCellSize = null) : Collider
{
    private Tilemap tilemap;
    private readonly Dictionary<Coord, HashSet<TileEntity>> spatialPartition = new();

    private readonly Coord cellSize = spatialPartitionCellSize ?? Coord.One * 8;

    private IEnumerable<Coord> GetCellIndices(TileEntityCollider collider)
    {
        return GetCellIndices(GetColliderBoundsAtPos(collider, collider.OwnerEntity.Position));
    }

    private IEnumerable<Coord> GetCellIndices(CoordBounds bounds)
    {
        var topLeft = bounds.TopLeft / cellSize;
        var bottomRight = bounds.BottomRight / cellSize;

        for (var y = topLeft.Y; y <= bottomRight.Y; y++)
        {
            for (var x = topLeft.X; x <= bottomRight.X; x++)
            {
                yield return new Coord(x, y);
            }
        }
    }

    protected override void OnAttach()
    {
        tilemap = GetComponent<Tilemap>();
        tilemap.TileEntityAdded += OnTileEntityAdded;
        tilemap.TileEntityRemoved += OnTileEntityRemoved;
    }
    
    private void OnTileEntityAdded(TileEntity tileEntity)
    {
        if (spatialPartitionCellSize is null) return;
        
        var collider = tileEntity.GetComponent<TileEntityCollider>();
        if (collider is null) return;

        foreach (var coord in GetCellIndices(collider))
        {
            spatialPartition.TryAdd(coord, []);
            spatialPartition[coord].Add(tileEntity);
        }
        
        tileEntity.PositionUpdated += OnTileEntityPositionUpdated;
    }

    private void OnTileEntityPositionUpdated(TileEntity sender, Coord oldPos, Coord newPos)
    {
        if (spatialPartitionCellSize is null) return;
        
        if (oldPos == newPos)
            return;
        
        var collider = sender.GetComponent<TileEntityCollider>();
        var oldIndices = GetCellIndices(GetColliderBoundsAtPos(collider, oldPos));
        var newIndices = GetCellIndices(GetColliderBoundsAtPos(collider, newPos));

        foreach (var index in oldIndices)
        {
            if (spatialPartition.TryGetValue(index, out var tileEntities))
            {
                tileEntities.Remove(sender);
                if (spatialPartition[index].Count == 0) spatialPartition.Remove(index);
            }
        }

        foreach (var index in newIndices)
        {
            if (!spatialPartition.ContainsKey(index)) spatialPartition.Add(index, []);
            spatialPartition[index].Add(sender);
        }
    }

    private void OnTileEntityRemoved(TileEntity tileEntity)
    {
        if (spatialPartitionCellSize is null) return;
        
        var collider = tileEntity.GetComponent<TileEntityCollider>();
        if (collider is null) return;
        
        foreach (var coord in GetCellIndices(collider))
        {
            spatialPartition[coord]?.Remove(tileEntity);
            if (spatialPartition[coord].Count == 0) spatialPartition.Remove(coord);
        }

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
        var cellIndices = GetCellIndices(collider);

        var tileEntitiesToCheck = spatialPartitionCellSize is null
            ? tilemap.TileEntities
            : cellIndices.SelectMany(index => spatialPartition.TryGetValue(index, out var value) ? value : []);
        
        foreach (var entity in tileEntitiesToCheck)
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