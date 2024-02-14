using System;
using System.Collections.Generic;
using System.Linq;
using MariEngine.Logging;
using Microsoft.Xna.Framework;

namespace MariEngine.Utils;

public class Random : RandomBase
{
    private System.Random random;
    private int seed;
    
    public Random()
    {
        random = new System.Random();
        CalculatePermutation();
        CalculateGradients();
    }

    public Random(int seed)
    {
        this.seed = seed;
        random = new System.Random(seed);
        CalculatePermutation();
        CalculateGradients();
    }

    public override RandomBase Seed(int seed)
    {
        this.seed = seed;
        random = new System.Random(seed);
        CalculatePermutation();
        CalculateGradients();
        return this;
    }
    
    public override int Next() => random.Next();
    public override int Next(int maxValue) => random.Next(maxValue);
    public override int Next(int minInclusive, int maxExclusive) => random.Next(minInclusive, maxExclusive);
    public override float NextFloat() => random.NextSingle();
    public override float NextFloat(float maxValue) => random.NextSingle() * maxValue;

    public override float NextFloat(float minInclusive, float maxExclusive) =>
        NextFloat(maxExclusive - minInclusive) + minInclusive;
    
    public override IList<T> Shuffle<T>(IList<T> list)
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
    public override TItem PickWeighted<TItem>(ICollection<(TItem item, float weight)> items, out bool picked, bool remove = false)
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