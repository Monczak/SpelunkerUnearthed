using MariEngine.Rendering;
using MariEngine.Sprites;

namespace MariEngine.Collision;

public class SpriteTileEntityCollider : TileEntityCollider
{
    private Sprite sprite;

    protected override void OnAttach()
    {
        sprite = OwnerEntity.GetComponent<TileEntitySpriteRenderer>().Sprite;
    }

    public override CoordBounds BoundingBox => new(-(sprite.Size - Coord.One) / 2, sprite.Size);
    public override CollisionGroup? GetCollisionGroup(Coord localPos) => sprite.Tiles[localPos]?.CollisionGroup;
}