using System.Collections.Generic;

namespace MariEngine.Components;

public class PriorityComparer<T> : IComparer<T> where T : IPriorityItem
{
    public int Compare(T x, T y)
    {
        return x?.Priority > y?.Priority ? 1 : -1; // Returning 0 for equal would result in components not being inserted
                                                   // to the SortedSet as they'd be considered duplicates
    }
}