using System.Collections.Generic;
using MariEngine.Loading;
using Microsoft.Xna.Framework;

namespace SpelunkerUnearthed.Scripts.MapGeneration.ParameterProviders;

public static class ParseUtils
{
    public static bool GetStringOrDefault(Dictionary<string, object> data, string key, out string s,
        bool required = true, string defaultValue = null)
    {
        if (!data.ContainsKey(key))
        {
            if (required)
                throw new ResourceLoadingException($"Value for {key} required, but not provided.");
            s = defaultValue;
            return true;
        }

        s = data[key] as string;
        return s is not null;
    }
    
    public static bool TryParseFloat(Dictionary<string, object> data, string key, out float f, bool required = true)
    {
        if (!data.ContainsKey(key))
        {
            if (required)
                throw new ResourceLoadingException($"Value for {key} required, but not provided.");
            f = default;
            return false;
        }
        
        if (!float.TryParse(data["Value"] as string, out f))
            throw new ResourceLoadingException($"Invalid value for key {key}.");

        return true;
    }
    
    public static bool TryParseVector2(Dictionary<string, object> data, string key, out Vector2 v, bool required = true)
    {
        if (!data.ContainsKey(key))
        {
            if (required)
                throw new ResourceLoadingException($"Value for {key} required, but not provided.");
            v = default;
            return false;
        }

        string str = data[key] as string;
        var vStr = str?.Split(" ");
        if (vStr is null)
        {
            v = default;
            return false;
        }

        if (!float.TryParse(vStr[0], out var vx) || !float.TryParse(vStr[1], out var vy))
        {
            v = default;
            return false;
        }
        
        v = new Vector2(vx, vy);
        return true;
    }
}