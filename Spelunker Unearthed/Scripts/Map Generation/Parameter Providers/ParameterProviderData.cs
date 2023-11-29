using System.Collections.Generic;

namespace SpelunkerUnearthed.Scripts.MapGeneration.ParameterProviders;

public struct ParameterProviderData
{
    public string Type { get; private set; }
    public Dictionary<string, object> Options { get; private set; }
}