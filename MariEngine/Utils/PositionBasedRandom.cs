using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MariEngine.Logging;
using Microsoft.Xna.Framework;

namespace MariEngine.Utils;

public sealed class PositionBasedRandom : RandomBase
{
    private int seed;
    private ThreadLocal<Coord> position = new();

    public PositionBasedRandom()
    {
        CalculatePermutation();
        CalculateGradients();
    }
    
    public PositionBasedRandom WithPosition(Coord position)
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
    
    protected override void CalculatePermutation()
    {
        Permutation = Enumerable.Range(0, 256).ToArray();
        var random = new System.Random(seed);
        
        for (var i = 0; i < Permutation.Length; i++)
        {
            var source = random.Next(Permutation.Length);

            (Permutation[i], Permutation[source]) = (Permutation[source], Permutation[i]);
        }
    }

    protected override void CalculateGradients()
    {
        Gradients = new Vector2[256];
        var random = new System.Random(seed);
        
        for (var i = 0; i < Gradients.Length; i++)
        {
            Vector2 gradient = new(random.NextSingle() * 2 - 1, random.NextSingle() * 2 - 1);
            gradient.Normalize();

            Gradients[i] = gradient;
        }
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