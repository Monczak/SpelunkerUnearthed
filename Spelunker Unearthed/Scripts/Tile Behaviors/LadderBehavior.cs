using MariEngine;
using MariEngine.Events;
using MariEngine.Logging;
using MariEngine.Services;
using MariEngine.Tiles;

namespace SpelunkerUnearthed.Scripts.TileBehaviors;

public class LadderBehavior : TileBehavior
{
    public override void OnSteppedOn(Tile tile, Coord position, TileEntity steppingEntity)
    {
        base.OnSteppedOn(tile, position, steppingEntity);
        ServiceRegistry.Get<EventManager>().Notify("TriggerWarp", position);
    }
}