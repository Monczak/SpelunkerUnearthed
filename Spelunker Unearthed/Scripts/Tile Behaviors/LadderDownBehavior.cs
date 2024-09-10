using MariEngine;
using MariEngine.Logging;
using MariEngine.Tiles;

namespace SpelunkerUnearthed.Scripts.TileBehaviors;

public class LadderDownBehavior : TileBehavior
{
    public override void OnSteppedOn(Coord position, TileEntity steppingEntity)
    {
        base.OnSteppedOn(position, steppingEntity);
        Logger.LogDebug($"Stepped on by {steppingEntity.Name}");
    }
}