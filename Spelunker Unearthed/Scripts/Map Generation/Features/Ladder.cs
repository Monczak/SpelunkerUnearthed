using MariEngine;
using MariEngine.Services;
using MariEngine.Tiles;

namespace SpelunkerUnearthed.Scripts.MapGeneration.Features;

public class Ladder(bool isDown) : ProceduralFeature
{
    public override CoordBounds Bounds { get; protected set; }
    
    protected override TileBuffer GenerateFeature(TileBuffer map)
    {
        var buffer = new TileBuffer(1, 1);
        buffer[Coord.Zero] = ServiceRegistry.Get<TileLoader>().Get(isDown ? "LadderDown" : "LadderUp");

        Bounds = new CoordBounds(Coord.Zero, Coord.One);

        return buffer;
    }
}