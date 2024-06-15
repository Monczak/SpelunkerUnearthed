using System;
using System.Collections.Generic;
using System.Linq;
using MariEngine;
using MariEngine.Audio;
using MariEngine.Debugging;
using MariEngine.Logging;
using MariEngine.Services;
using MariEngine.Tiles;
using MariEngine.Utils;
using Microsoft.Xna.Framework;

namespace SpelunkerUnearthed.Scripts.Audio;

public class WorldReverbTrait(Tilemap tilemap) : WorldAudioTrait(tilemap)
{
    private const int EnvironmentEvaluationRayCount = 24;
    private const int EnvironmentEvaluationRayBounceCount = 8;

    private IEnumerable<Raycasting.Ray> GetEnvironmentEvaluationRays(Vector2 origin)
    {
        for (var i = 0; i < EnvironmentEvaluationRayCount; i++)
        {
            var rayAngle = (float)i / EnvironmentEvaluationRayCount * MathF.Tau;
            var ray = new Raycasting.Ray((Vector2)tilemap.WorldPointToCoord(origin) + Vector2.One * 0.5f,
                new Vector2(MathF.Cos(rayAngle), MathF.Sin(rayAngle)));
            yield return ray;
        }
    }

    private IEnumerable<(Raycasting.Ray, Raycasting.HitInfo)> TraceBounces(Raycasting.Ray ray)
    {
        var currentRay = ray;
        for (var i = 0; i < EnvironmentEvaluationRayBounceCount; i++)
        {
            var hitInfo = Raycasting.Raycast(tilemap.GetLayer(Tilemap.BaseLayer), currentRay);
            if (hitInfo is null)
                yield break;

            yield return (currentRay, hitInfo.Value);

            currentRay = new Raycasting.Ray(hitInfo.Value.Position,
                Vector2.Reflect(currentRay.Direction, hitInfo.Value.Normal));
        }
    }
    
    protected override void Apply(AudioEvent audioEvent)
    {
        var rayEnergies = new float[EnvironmentEvaluationRayCount];
        var bounceDistances = new float[EnvironmentEvaluationRayCount];
        var firstBounceDistances = new float[EnvironmentEvaluationRayCount];
        var raysHit = 0;
        var rayIndex = 0;
        foreach (var ray in GetEnvironmentEvaluationRays(audioEvent.Position))
        {
            var bounce = 0;
            rayEnergies[rayIndex] = 1;
            bounceDistances[rayIndex] = 0;
            foreach (var (subRay, hitInfo) in TraceBounces(ray))
            {
                var origin = tilemap.Vector2ToWorldPoint(subRay.Origin);
                var hitPoint = tilemap.Vector2ToWorldPoint(hitInfo.Position);
                
                Gizmos.Default.DrawLine(origin, hitPoint, new Color(1f / (bounce + 1), 1f / (bounce + 1), 1f / (bounce + 1), 1), lifetime: 1);
                Gizmos.Default.DrawLine(hitPoint, hitPoint + hitInfo.Normal, Color.Magenta, lifetime: 1);
                foreach (var debugPoint in hitInfo.DebugPoints)
                {
                    Gizmos.Default.DrawRectangle(tilemap.Vector2ToWorldPoint(debugPoint), Vector2.One * 0.2f, Color.Green, lifetime: 1);
                }

                bounceDistances[rayIndex] += hitInfo.Distance / EnvironmentEvaluationRayBounceCount;
                if (bounce == 0) firstBounceDistances[rayIndex] = bounceDistances[rayIndex] * EnvironmentEvaluationRayBounceCount;
                rayEnergies[rayIndex] *= MathF.Pow(hitInfo.Tile.Material.SoundReflectivity, 1f / EnvironmentEvaluationRayBounceCount);
                bounce++;
            }

            if (bounce > 0) raysHit++;
            rayIndex++;
        }

        if (raysHit == 0)
            return;

        var avgBouncePower = bounceDistances.Select((d, i) => d * rayEnergies[i]).Average();
        var distGeoMean = firstBounceDistances.GeometricMean();
        // Logger.LogDebug($"Avg bounce power: {avgBouncePower}");
        // Logger.LogDebug($"Distances: {string.Join(", ", bounceDistances)}");
        // Logger.LogDebug($"EarlyLate ({distGeoMean}): {string.Join(", ", firstBounceDistances)}");
        
        var reverbTime = MathF.Pow(MathUtils.Clamp(avgBouncePower / 50, 0, 1), 0.3f);
        var reverbEarlyLate = MathUtils.Lerp(0.3f, 0.6f, MathF.Pow(MathUtils.InverseLerp(0, 10, distGeoMean), 0.5f));
        var reverbWet = MathUtils.Remap(0.3f, 0.6f, 0.1f, 0.4f, reverbEarlyLate);
        
        // Logger.LogDebug($"Time: {reverbTime} Wet: {reverbWet} EarlyLate: {reverbEarlyLate}");
        
        audioEvent.SetParameterValue("Reverb Time", reverbTime);
        audioEvent.SetParameterValue("Reverb Wet", reverbWet);
        audioEvent.SetParameterValue("Reverb EarlyLate", reverbEarlyLate);
    }
}