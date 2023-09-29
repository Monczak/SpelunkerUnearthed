using System;
using System.Collections.Generic;
using System.Linq;
using MariEngine.Logging;
using MariEngine.Utils;

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
    public float NextFloat(float maxValue) => random.NextSingle() * maxValue;

    public float NextFloat(float minInclusive, float maxExclusive) =>
        NextFloat(maxExclusive - minInclusive) + minInclusive;
    
    public void Shuffle<T>(IList<T> list)
    {
        int n = list.Count;  
        while (n > 1) 
        {  
            n--;  
            int k = Next(n + 1);  
            (list[k], list[n]) = (list[n], list[k]);
        }
    }
    
    // TODO: What to do with negative weights?
    public TItem PickWeighted<TItem>(ICollection<(TItem item, float weight)> items, out bool picked, bool remove = false)
    {
        float minWeight = float.PositiveInfinity, maxWeight = float.NegativeInfinity;
        
        foreach (var (item, weight) in items)
        {
            if (weight < 0) Logger.LogWarning($"Weight lower than 0! {item} - {weight}");
            
            minWeight = weight < minWeight ? weight : minWeight;
            maxWeight = weight > maxWeight ? weight : maxWeight;
        }
        
        float weightSum = 0;
        foreach (var (item, weight) in items)
        {
            weightSum += weight;
        }

        float x = NextFloat(weightSum);
        foreach (var (item, weight) in items)
        {
            if (weight == 0) continue;
            
            if (x < weight)
            {
                if (remove) items.Remove((item, weight));
                picked = true;
                return item;
            }
            x -= weight;
        }

        picked = false;
        return default; // Should never get here

        float NormalizeWeight(float weight)
        {
            // return weight;
            return MathUtils.InverseLerp(minWeight, maxWeight, weight);
        }
    }
}