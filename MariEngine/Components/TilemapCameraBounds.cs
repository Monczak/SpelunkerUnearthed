using MariEngine.Logging;
using MariEngine.Rendering;
using MariEngine.Tiles;
using Microsoft.Xna.Framework;

namespace MariEngine.Components;

public class TilemapCameraBounds : CameraBounds
{
    private Tilemap tilemap;
    private TilemapRenderer tilemapRenderer;

    public override Bounds GetBounds() => Bounds.MakeCorners(tilemapRenderer.CoordToWorldPoint((Coord)tilemap.Bounds.TopLeft),
        tilemapRenderer.CoordToWorldPoint((Coord)tilemap.Bounds.BottomRight));

    public override void OnAttach()
    {
        base.OnAttach();
        tilemap = GetComponent<Tilemap>();
        tilemapRenderer = GetComponent<TilemapRenderer>();
    }
}