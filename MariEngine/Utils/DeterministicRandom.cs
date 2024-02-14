using System;
using System.Collections.Generic;
using System.Threading;
using MariEngine.Logging;

namespace MariEngine.Utils;

public class DeterministicRandom : RandomBase
{
    private int seed;
    private ThreadLocal<Coord> position = new();

    public DeterministicRandom()
    {
        CalculatePermutation();
        CalculateGradients();
    }
    
    public DeterministicRandom WithPosition(Coord position)
    {
        this.position.Value = position;
        return this;
    }

    public override RandomBase Seed(int seed)
    {
        this.seed = seed;
        CalculatePermutation();
        CalculateGradients();
        return this;
    }

    public override int Next()
    {
        return PseudoRandomUtils.Hash(position.Value.X ^ PseudoRandomUtils.Hash(position.Value.Y)) ^ PseudoRandomUtils.Hash(seed);
    }

    public override int Next(int maxValue)
    {
        return Next() % maxValue;
    }

    public override int Next(int minInclusive, int maxExclusive)
    {
        return Next(maxExclusive - minInclusive) + minInclusive;
    }

    public override float NextFloat()
    {
        return (float)Next() / int.MaxValue;
    }

    public override float NextFloat(float maxValue)
    {
        return NextFloat() * maxValue;
    }
    
    public override float NextFloat(float minInclusive, float maxExclusive)
    {
        return NextFloat(maxExclusive - minInclusive) + minInclusive;
    }

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