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
        List<Coord> candidates = new();

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
    
    public static List<Coord> WalkTo(Coord startPos, Coord goalPos, Properties properties, WeightChooser weightChooser, Random random = null, int pathLengthLimit = 1000)
    {
        random ??= ServiceRegistry.Get<RandomProvider>().Request(Constants.GeneralPurposeRng);
        
        Coord currentPos = startPos;
        HashSet<Coord> path = new() { currentPos };

        while (currentPos != goalPos)
        {
            // TODO: Optimize this to reduce memory allocations
            var candidates = FindCandidates(currentPos, properties.CanGoDiagonally)
                .Select(pos => (pos, weight: MathF.Pow(-(goalPos - pos).SqrMagnitude * properties.DistanceCost + weightChooser(pos) + -GetDeviation(startPos, goalPos, pos) * properties.DeviationPenalty, 1 / properties.Variance)))
                .ToList();

            var minWeight = candidates.MinBy(pair => pair.weight).weight;
            var maxWeight = candidates.MaxBy(pair => pair.weight).weight;

            candidates =
                candidates.Select(pair => (pair.pos, MathUtils.InverseLerp(minWeight, maxWeight, pair.weight))).ToList();

            if (!properties.CanSelfIntersect)
                candidates = candidates.Where(pair => !path.Contains(pair.pos)).ToList();

            currentPos = random.PickWeighted(candidates, out _);
            path.Add(currentPos);

            if (path.Count > pathLengthLimit)
            {
                Logger.LogWarning($"Maximum path length exceeded: from {startPos} to {goalPos}, {path.Count} positions in path");
                break;
            }
        }

        return path.ToList();
    }
}