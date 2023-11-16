using System;
using System.Collections.Generic;
using System.Linq;
using MariEngine.Logging;
using MariEngine.Utils;
using Microsoft.Xna.Framework;
using Random = MariEngine.Utils.Random;

namespace MariEngine.Services;

public class RandomProvider : Service
{
    private readonly Random globalRandom = new();

    private readonly Dictionary<string, Random> localRandoms = new();

    public void Seed(int seed) => globalRandom.Seed(seed);
    public int Next() => globalRandom.Next();
    public int Next(int maxValue) => globalRandom.Next(maxValue);
    public int Next(int minInclusive, int maxExclusive) => globalRandom.Next(minInclusive, maxExclusive);
    public float NextFloat() => globalRandom.NextFloat();
    public float NextFloat(float maxValue) => globalRandom.NextFloat() * maxValue;

    public float NextFloat(float minInclusive, float maxExclusive) =>
        NextFloat(maxExclusive - minInclusive) + minInclusive;

    public Random Request(string name)
    {
        if (localRandoms.TryGetValue(name, out var random)) return random;
        
        localRandoms[name] = new Random();
        return localRandoms[name];
    }
}