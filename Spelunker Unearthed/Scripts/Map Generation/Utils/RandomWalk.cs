using System;
using System.Collections.Generic;
using System.Linq;
using MariEngine;
using MariEngine.Logging;
using MariEngine.Services;
using MariEngine.Utils;
using Microsoft.Xna.Framework;
using Random = MariEngine.Utils.Random;

namespace SpelunkerUnearthed.Scripts.MapGeneration.Utils;

public static class RandomWalk
{
    public record struct Properties(float Variance, float DeviationPenalty, float DistanceCost, bool CanSelfIntersect, bool CanGoDiagonally = false);

    public delegate float WeightChooser(Coord pos);

    private static List<Coord> FindCandidates(Coord pos, bool diagonalsAllowed = false)
    {
        List<Coord> candidates = [];

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx * dy != 0 && !diagonalsAllowed) continue;
                if (dx == 0 && dy == 0) continue;
                
                candidates.Add(new Coord(pos.X + dx, pos.Y + dy));
            }
        }

        return candidates;
    }

    private static float GetDeviation(Coord startPos, Coord goalPos, Coord currentPos)
    {
        Vector2 start = (Vector2)startPos;
        Vector2 end = (Vector2)goalPos;
        Vector2 point = (Vector2)currentPos;

        return MathF.Abs(MathUtils.Cross(end - start, point - end)) / (end - start).Length();
    }
    
    // TODO: Fix self intersection prevention potentially making it impossible to reach the goal pos
    public static List<Coord> WalkTo(Coord startPos, Coord goalPos, Properties properties, WeightChooser weightChooser, RandomBase random = null, int pathLengthLimit = 1000)
    {
        random ??= ServiceRegistry.Get<RandomProvider>().Request(Constants.GeneralPurposeRng);
        
        Coord currentPos = startPos;
        List<Coord> path = [currentPos];

        while (currentPos != goalPos)
        {
            // TODO: Optimize this to reduce memory allocations
            var candidates = FindCandidates(currentPos, properties.CanGoDiagonally)
                .Select(pos => (pos, weight: MathF.Pow(-(goalPos - pos).SqrMagnitude * properties.DistanceCost + weightChooser(pos) + -GetDeviation(startPos, goalPos, pos) * properties.DeviationPenalty, 1 / properties.Variance)))
                .ToList();

            var minWeight = candidates.MinBy(pair => pair.weight).weight;
            var maxWeight = candidates.MaxBy(pair => pair.weight).weight;

            candidates =
                candidates.Select(pair => (pair.pos, MathUtils.InverseLerp(minWeight, maxWeight, pair.weight) + 1)).ToList();

            if (!properties.CanSelfIntersect)
                candidates = candidates.Where(pair => !path.Contains(pair.pos)).ToList();

            if (random is PositionBasedRandom deterministicRandom)
                random = deterministicRandom.WithPosition(currentPos);
            currentPos = random.PickWeighted(candidates, out var picked);
            if (!picked)
            {
                Logger.LogWarning($"Something went really wrong while walking from {startPos} to {goalPos}, trying again");
                currentPos = startPos;
                path = [currentPos];
            }
            path.Add(currentPos);

            if (path.Count > pathLengthLimit)
            {
                Logger.LogWarning($"Maximum path length exceeded: from {startPos} to {goalPos}, {path.Count} positions in path");
                break;
            }
        }

        return path;
    }
}