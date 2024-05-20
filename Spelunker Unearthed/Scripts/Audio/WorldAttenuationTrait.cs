using System;
using MariEngine;
using MariEngine.Audio;
using MariEngine.Services;
using MariEngine.Tiles;
using MariEngine.Utils;
using Microsoft.Xna.Framework;

namespace SpelunkerUnearthed.Scripts.Audio;

public class WorldAttenuationTrait(Tilemap tilemap) : AudioTrait
{
    protected override void Apply(AudioEvent audioEvent)
    {
        var emitterTilemapPos = tilemap.WorldPointToCoord(audioEvent.Position);
        var listenerTilemapPos = tilemap.WorldPointToCoord(ServiceRegistry.Get<AudioManager>().Listener.GetPosition());

        // TODO: Use raytracing to evaluate attenuation 
        var cumulativeAttenuation = 0f;
        foreach (var coord in DrawingUtils.BresenhamLine(emitterTilemapPos, listenerTilemapPos))
        {
            if (!tilemap.IsInBounds(coord)) 
                continue;
            
            cumulativeAttenuation += 1 - (tilemap.Get(coord, Tilemap.BaseLayer)?.Material.SoundTransmittance ?? 1);
        }
        
        audioEvent.SetParameterValue("Attenuation", MathUtils.Clamp(cumulativeAttenuation, 0, 1));
    }
}