using System.Collections.Generic;

namespace SpelunkerUnearthed.Scripts.MapGeneration.Features;

public record struct FeatureData(string Name, List<string> Structure, Dictionary<string, string> Keys);