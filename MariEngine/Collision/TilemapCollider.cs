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
        
        Tile mapTile = tilemap[coord];
        return DoTilesCollide(mapTile, tileEntity.Tile);
    }
}