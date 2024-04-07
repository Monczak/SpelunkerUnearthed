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
        var emitterTilemapPos = (Coord)GetPosition();
        var listenerTilemapPos = tilemap.WorldPointToCoord(ServiceRegistry.Get<AudioManager>().Listener.GetPosition());

        var cumulativeAttenuation = 0f;
        foreach (var coord in DrawingUtils.BresenhamLine(emitterTilemapPos, listenerTilemapPos))
        {
            if (!tilemap.IsInBounds(coord)) 
                continue;
            
            cumulativeAttenuation += 1 - tilemap.Get(coord, Tilemap.BaseLayer).Material.SoundTransmittance;
        }
        
        audioEvent.SetParameterValue("Attenuation", MathUtils.Clamp(cumulativeAttenuation, 0, 1));
    }
}