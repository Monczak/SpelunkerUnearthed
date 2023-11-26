using System.Collections.Generic;

namespace SpelunkerUnearthed.Scripts.MapGeneration.TileProviders;

public struct ParameterProviderData
{
    public string Type { get; private set; }
    public Dictionary<string, string> Options { get; private set; }
}