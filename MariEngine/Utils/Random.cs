using System.Collections.Generic;
using MariEngine.Logging;

namespace MariEngine.Utils;

public class Random
{
    private System.Random random;

    public Random()
    {
        random = new System.Random();
    }

    public Random(int seed)
    {
        random = new System.Random(seed);
    }

    public Random Seed(int seed)
    {
        random = new System.Random(seed);
        return this;
    }
    
    public int Next() => random.Next();
    public int Next(int maxValue) => random.Next(maxValue);
    public int Next(int minInclusive, int maxExclusive) => random.Next(minInclusive, maxExclusive);
    public float NextFloat() => random.NextSingle();
    public float NextFloat(float maxValue) => random.NextSingle() * maxValue;

    public float NextFloat(float minInclusive, float maxExclusive) =>
        NextFloat(maxExclusive - minInclusive) + minInclusive;
    
    public IList<T> Shuffle<T>(IList<T> list)
    {
        int n = list.Count;  
        while (n > 1) 
        {  
            n--;  
            int k = Next(n + 1);  
            (list[k], list[n]) = (list[n], list[k]);
        }

        return list;
    }
    
    // TODO: What to do with negative weights?
    public TItem PickWeighted<TItem>(ICollection<(TItem item, float weight)> items, out bool picked, bool remove = false)
    {
        float weightSum = 0;
        foreach (var (item, weight) in items)
        {
            if (weight < 0) Logger.LogWarning($"Weight lower than 0! {item} - {weight}");
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
        return default;
    }
}