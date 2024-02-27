using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace MariEngine.Utils;

public abstract class RandomBase
{
    public abstract RandomBase Seed(int seed);
    public abstract int Next();
    public abstract int Next(int maxValue);
    public abstract int Next(int minInclusive, int maxExclusive);
    public abstract float NextFloat();
    public abstract float NextFloat(float maxValue);
    public abstract float NextFloat(float minInclusive, float maxExclusive);
    public abstract IList<T> Shuffle<T>(IList<T> list);
    public abstract TItem PickWeighted<TItem>(ICollection<(TItem item, float weight)> items, out bool picked, bool remove = false);

    protected int[] Permutation;
    protected Vector2[] Gradients;

    protected virtual void CalculatePermutation()
    {
        Permutation = Enumerable.Range(0, 256).ToArray();
        
        for (var i = 0; i < Permutation.Length; i++)
        {
            var source = Next(Permutation.Length);

            (Permutation[i], Permutation[source]) = (Permutation[source], Permutation[i]);
        }
    }

    protected virtual void CalculateGradients()
    {
        Gradients = new Vector2[256];

        for (var i = 0; i < Gradients.Length; i++)
        {
            Vector2 gradient = new(NextFloat() * 2 - 1, NextFloat() * 2 - 1);
            gradient.Normalize();

            Gradients[i] = gradient;
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

            var index = Permutation[((int)ij.X + Permutation.Length) % Permutation.Length];
            index = Permutation[(index + (int)ij.Y + Permutation.Length) % Permutation.Length];

            var grad = Gradients[index % Gradients.Length];

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