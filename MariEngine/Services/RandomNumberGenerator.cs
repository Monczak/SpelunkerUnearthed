using System;

namespace MariEngine.Services;

public class RandomNumberGenerator : Service
{
    private Random random = new();

    public int Next() => random.Next();
    public int Next(int maxValue) => random.Next(maxValue);
    public int Next(int minInclusive, int maxExclusive) => random.Next(minInclusive, maxExclusive);
    public float NextFloat() => random.NextSingle();
}