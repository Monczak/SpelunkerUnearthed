using System;
using System.Collections.Generic;
using MariEngine;
using MariEngine.Audio;
using MariEngine.Logging;
using MariEngine.Services;
using MariEngine.Tiles;
using MariEngine.Utils;
using Microsoft.Xna.Framework;

namespace SpelunkerUnearthed.Scripts.Audio;

public class WorldReverbTrait(Tilemap tilemap) : AudioTrait
{
    private const int EnvironmentEvaluationRayCount = 8;

    private List<Raycasting.Ray> GetEnvironmentEvaluationRays(Vector2 origin)
    {
        var rays = new List<Raycasting.Ray>();
        for (int i = 0; i < EnvironmentEvaluationRayCount; i++)
        {
            var rayAngle = (float)i / EnvironmentEvaluationRayCount * MathF.Tau;
            var ray = new Raycasting.Ray((Vector2)tilemap.WorldPointToCoord(origin),
                new Vector2(MathF.Cos(rayAngle), MathF.Sin(rayAngle)));
            rays.Add(ray);
        }

        return rays;
    } 
    
    protected override void Apply(AudioEvent audioEvent)
    {
        var reflectivitySum = 0f;
        var distanceProduct = 1f;
        var raysHit = 0;
        foreach (var ray in GetEnvironmentEvaluationRays(audioEvent.Position))
        {
            var hitInfo = Raycasting.Raycast(tilemap.GetLayer(Tilemap.BaseLayer), ray);

            var material = hitInfo is null ? ServiceRegistry.Get<MaterialLoader>().Get("None") : hitInfo.Value.Tile.Material;
            reflectivitySum += material.SoundReflectivity;
            distanceProduct *= hitInfo?.Distance ?? 1f;

            if (hitInfo is not null) raysHit++;
        }

        if (raysHit == 0)
            return;

        // TODO: Improve reverb parameter calc to make it more realistic
        var avgReflectivity = reflectivitySum / raysHit;
        var avgDistance = MathF.Pow(distanceProduct, 1f / raysHit);

        var reverbTime = MathF.Pow(avgReflectivity, 1.8f);
        var reverbEarlyLate = MathUtils.Clamp(MathUtils.InverseLerp(0, 10, avgDistance), 0, 1);
        var reverbWet = MathUtils.Clamp((reverbTime + reverbEarlyLate) / 3, 0, 0.4f);
        
        // Logger.LogDebug($"Time: {reverbTime} Wet: {reverbWet} EarlyLate: {reverbEarlyLate}");
        
        audioEvent.SetParameterValue("Reverb Time", reverbTime);
        audioEvent.SetParameterValue("Reverb Wet", reverbWet);
        audioEvent.SetParameterValue("Reverb EarlyLate", reverbEarlyLate);
    }
}