using System;
using System.Collections.Generic;

namespace MariEngine.Services;

public class RandomNumberGenerator : Service
{
    private Random random = new();

    public void Seed(int seed)
    {
        random = new Random(seed);
    }
    
    public int Next() => random.Next();
    public int Next(int maxValue) => random.Next(maxValue);
    public int Next(int minInclusive, int maxExclusive) => random.Next(minInclusive, maxExclusive);
    public float NextFloat() => random.NextSingle();

    public void Shuffle<T>(IList<T> list)
    {
        int n = list.Count;  
        while (n > 1) {  
            n--;  
            int k = Next(n + 1);  
            (list[k], list[n]) = (list[n], list[k]);
        }
    }
}