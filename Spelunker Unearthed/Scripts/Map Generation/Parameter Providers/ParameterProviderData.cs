using System.Collections.Generic;

namespace SpelunkerUnearthed.Scripts.MapGeneration.ParameterProviders;

public record struct ParameterProviderData(string Type, Dictionary<string, object> Options);