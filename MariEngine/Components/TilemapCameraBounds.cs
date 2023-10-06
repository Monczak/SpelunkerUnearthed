using MariEngine.Logging;
using MariEngine.Rendering;
using MariEngine.Tiles;
using Microsoft.Xna.Framework;

namespace MariEngine.Components;

public class TilemapCameraBounds : CameraBounds
{
    private Tilemap tilemap;
    private TilemapRenderer tilemapRenderer;

    public override Bounds GetBounds() => Bounds.MakeCorners(tilemapRenderer.CoordToWorldPoint(tilemap.Bounds.TopLeft) - Vector2.One * 0.5f,
        tilemapRenderer.CoordToWorldPoint(tilemap.Bounds.BottomRight) + Vector2.One * 0.5f);

    protected override void OnAttach()
    {
        base.OnAttach();
        tilemap = GetComponent<Tilemap>();
        tilemapRenderer = GetComponent<TilemapRenderer>();
    }
}