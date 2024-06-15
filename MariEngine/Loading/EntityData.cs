using System.Collections.Generic;

namespace MariEngine.Loading;

public class EntityData
{
    public Dictionary<string, ComponentData> Components { get; init; }
    public int Priority { get; init; } = 0;
}