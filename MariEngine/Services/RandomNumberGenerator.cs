using System;

namespace MariEngine.Services;

public class RandomNumberGenerator : Service
{
    private Random random = new();

    public int Next() => random.Next();
    public float NextFloat() => random.NextSingle();
}