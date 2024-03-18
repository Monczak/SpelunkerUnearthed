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
        if (!tilemap.IsInBounds(coord)) return true;
        
        // TODO: Use custom colliders
        // Tile mapTile = tilemap.Get(coord, Tilemap.BaseLayer);   // TODO: Handle collision on other layers?
        // return DoTilesCollide(mapTile, tileEntity.Tile);
        return false;
    }
}