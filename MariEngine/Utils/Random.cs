using System;
using System.Collections.Generic;
using System.Linq;
using MariEngine.Logging;
using Microsoft.Xna.Framework;

namespace MariEngine.Utils;

public class Random
{
    private System.Random random;

    public Random()
    {
        random = new System.Random();
        CalculatePermutation(out permutation);
        CalculateGradients(out gradients);
    }

    public Random(int seed)
    {
        random = new System.Random(seed);
        CalculatePermutation(out permutation);
        CalculateGradients(out gradients);
    }

    public Random Seed(int seed)
    {
        random = new System.Random(seed);
        CalculatePermutation(out permutation);
        CalculateGradients(out gradients);
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
    
    private int[] permutation;

    private Vector2[] gradients;

    private void CalculatePermutation(out int[] p)
    {
        p = Enumerable.Range(0, 256).ToArray();
        
        for (var i = 0; i < p.Length; i++)
        {
            var source = Next(p.Length);

            (p[i], p[source]) = (p[source], p[i]);
        }
    }

    private void CalculateGradients(out Vector2[] grad)
    {
        grad = new Vector2[256];

        for (var i = 0; i < grad.Length; i++)
        {
            Vector2 gradient = new(NextFloat() * 2 - 1, NextFloat() * 2 - 1);
            gradient.Normalize();

            grad[i] = gradient;
        }

    }

    private float Fade(float t)
    {
        t = Math.Abs(t);
        return 1f - t * t * t * (t * (t * 6 - 15) + 10);
    }

    private float Q(float u, float v)
    {
        return Fade(u) * Fade(v);
    }

    private float Perlin(float x, float y)
    {
        var cell = new Vector2(MathF.Floor(x), MathF.Floor(y));

        var total = 0f;

        var corners = new[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0), new Vector2(1, 1) };

        foreach (var n in corners)
        {
            var ij = cell + n;
            var uv = new Vector2(x - ij.X, y - ij.Y);

            var index = permutation[((int)ij.X + permutation.Length) % permutation.Length];
            index = permutation[(index + (int)ij.Y + permutation.Length) % permutation.Length];

            var grad = gradients[index % gradients.Length];

            total += Q(uv.X, uv.Y) * Vector2.Dot(grad, uv);
        }

        return Math.Max(Math.Min(total, 1f), -1f);
    }
    
    public float Perlin(Vector2 input)
    {
        return Perlin(input.X, input.Y);
    }

    public float Perlin01(Vector2 input) => (Perlin(input) + 1) / 2;

    public (float value, int cellHash) Voronoi(Vector3 input, float cellSize)
    {
        Vector3 v = input / cellSize;
        Vector3 baseCell = Vector3.Floor(v);

        float minDist = 10;
        int minCellHash = 0;
        
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dz = -1; dz <= 1; dz++)
                {
                    Vector3 cell = new((int)baseCell.X + dx, (int)baseCell.Y + dy, (int)baseCell.Z + dz);
                
                    int x = PseudoRandomUtils.Hash((int)cell.X);
                    int y = PseudoRandomUtils.Hash(x + (int)cell.Y);
                    int z = PseudoRandomUtils.Hash(y + (int)cell.Z);
                    x = PseudoRandomUtils.Hash(z);
        
                    Vector3 cellPos = cell + new Vector3(
                        x / (float)int.MaxValue,  
                        y / (float)int.MaxValue,
                        z / (float)int.MaxValue
                    );

                    float distToCell = (v - cellPos).Length();
                    if (distToCell < minDist)
                    {
                        minDist = distToCell;
                        minCellHash = PseudoRandomUtils.Hash(x);
                    }
                }
                
            }
        }
        
        
        return (minDist, minCellHash);
    }
}