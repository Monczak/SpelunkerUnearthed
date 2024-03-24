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

        return ResolveTilemapCollision(collider, coord);
    }

    private bool ResolveTilemapCollision(TileEntityCollider collider, Coord coord)
    {
        if (!tilemap.IsInBounds(coord)) return true;
        
        var colliderBounds = new CoordBounds(coord + collider.BoundingBox.TopLeft, collider.BoundingBox.Size);

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
}