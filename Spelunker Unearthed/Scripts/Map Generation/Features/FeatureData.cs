using System.Collections.Generic;

namespace SpelunkerUnearthed.Scripts.MapGeneration.Features;

public struct FeatureData
{
    public string Name { get; private set; }
    public List<string> Structure { get; private set; }
    public Dictionary<string, string> Keys { get; private set; }
}