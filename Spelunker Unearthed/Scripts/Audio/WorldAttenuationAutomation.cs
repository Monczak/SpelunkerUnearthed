using MariEngine.Audio;
using MariEngine.Services;
using MariEngine.Tiles;
using MariEngine.Utils;

namespace SpelunkerUnearthed.Scripts.Audio;

public class WorldAttenuationAutomation(TileEntity emitter) : AudioParameterAutomation
{
    protected override void Apply(AudioEvent audioEvent)
    {
        var emitterTilemapPos = emitter.Position;
        var listenerTilemapPos = emitter.Tilemap.WorldPointToCoord(ServiceRegistry.Get<AudioManager>().Listener.GetPosition());

        var cumulativeAttenuation = 0f;
        foreach (var coord in DrawingUtils.BresenhamLine(emitterTilemapPos, listenerTilemapPos))
        {
            if (!emitter.Tilemap.IsInBounds(coord)) 
                continue;
            
            cumulativeAttenuation += 1 - emitter.Tilemap.Get(coord, Tilemap.BaseLayer).Material.SoundTransmittance;
        }
        
        audioEvent.SetParameterValue("Attenuation", MathUtils.Clamp(cumulativeAttenuation, 0, 1));
    }
}