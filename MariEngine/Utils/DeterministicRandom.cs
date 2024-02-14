using System.Collections.Generic;
using MariEngine.Logging;

namespace MariEngine.Utils;

public class DeterministicRandom : IRandom
{
    private int seed;
    private Coord position;

    public DeterministicRandom WithPosition(Coord position)
    {
        this.position = position;
        return this;
    }

    public IRandom Seed(int seed)
    {
        this.seed = seed;
        return this;
    }

    public int Next()
    {
        return PseudoRandomUtils.Hash(position.X ^ PseudoRandomUtils.Hash(position.Y)) ^ PseudoRandomUtils.Hash(seed);
    }

    public int Next(int maxValue)
    {
        return Next() % maxValue;
    }

    public int Next(int minInclusive, int maxExclusive)
    {
        return Next(maxExclusive - minInclusive) + minInclusive;
    }

    public float NextFloat()
    {
        return (float)Next() / int.MaxValue;
    }

    public float NextFloat(float maxValue)
    {
        return NextFloat() * maxValue;
    }
    
    public float NextFloat(float minInclusive, float maxExclusive)
    {
        return NextFloat(maxExclusive - minInclusive) + minInclusive;
    }

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