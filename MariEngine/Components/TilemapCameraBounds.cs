using MariEngine.Logging;
using MariEngine.Rendering;
using MariEngine.Tiles;
using Microsoft.Xna.Framework;

namespace MariEngine.Components;

public class TilemapCameraBounds : CameraBounds
{
    private Tilemap tilemap;
    private TilemapRenderer tilemapRenderer;

    public override Bounds GetBounds() => Bounds.MakeCorners(tilemap.CoordToWorldPoint(tilemap.Bounds.TopLeft),
        tilemap.CoordToWorldPoint(tilemap.Bounds.BottomRight));

    protected override void OnAttach()
    {
        base.OnAttach();
        tilemap = GetComponent<Tilemap>();
        tilemapRenderer = GetComponent<TilemapRenderer>();
    }
}