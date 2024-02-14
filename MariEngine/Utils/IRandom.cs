using System.Collections.Generic;

namespace MariEngine.Utils;

public interface IRandom
{
    IRandom Seed(int seed);
    int Next();
    int Next(int maxValue);
    int Next(int minInclusive, int maxExclusive);
    float NextFloat();
    float NextFloat(float maxValue);
    float NextFloat(float minInclusive, float maxExclusive);

    IList<T> Shuffle<T>(IList<T> list);
    TItem PickWeighted<TItem>(ICollection<(TItem item, float weight)> items, out bool picked, bool remove = false);
}