using System;
using System.Collections.Generic;
using System.Linq;

namespace MariEngine;

public static class Tags
{
    private static readonly Dictionary<string, int> TagValues = new();
    private const int TagCountLimit = 32;
    
    public static int GetValue(string tag)
    {
        if (TagValues.Count == 0)
            TagValues[tag] = 1;

        if (TagValues.Count >= TagCountLimit)
            throw new Exception($"Cannot fit tag {tag} - no slot available.");

        TagValues.TryAdd(tag, TagValues.Values.Max() << 1);
        return TagValues[tag];
    }

    public static bool HasTag(int tags, string tagToCheck)
    {
        return (tags & GetValue(tagToCheck)) != 0;
    }
}