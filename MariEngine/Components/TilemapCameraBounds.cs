using MariEngine.Logging;
using MariEngine.Rendering;
using MariEngine.Tiles;
using Microsoft.Xna.Framework;

namespace MariEngine.Components;

public class TilemapCameraBounds : CameraBounds
{
    private Tilemap tilemap;

    public override Bounds GetBounds() => Bounds.MakeCorners(tilemap.CoordToWorldPoint(tilemap.Bounds.TopLeft),
        tilemap.CoordToWorldPoint(tilemap.Bounds.BottomRight));

    protected internal override void Initialize()
    {
        base.Initialize();
        tilemap = GetComponent<Tilemap>();
    }
}