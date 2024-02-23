using System;
using System.Collections.Generic;
using System.Linq;

namespace MariEngine.Utils;

internal static class ShortKeyGen
{
    private const string KeyChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
    
    public static Dictionary<T, string> GetKeys<T>(IEnumerable<T> items)
    {
        Dictionary<T, string> keys = new();

        var itemList = items.ToList();
        foreach (var item in itemList) keys.TryAdd(item, "");
        var keyLength = (int)Math.Ceiling(Math.Log(keys.Count + 0.01, KeyChars.Length));
        var keyIndexes = new int[keyLength];

        foreach (var item in keys.Keys)
        {
            keys[item] = string.Join("", keyIndexes.Select(j => KeyChars[j]));
            keyIndexes[^1]++;
            for (var j = keyIndexes.Length - 1; j >= 0; j--)
            {
                if (keyIndexes[j] >= KeyChars.Length)
                {
                    keyIndexes[j] = 0;
                    if (j - 1 < 0)
                        throw new InvalidOperationException("Not enough key characters to encode item.");
                    keyIndexes[j - 1]++;
                }
            }
        }
        
        return keys;
    }
}