using System.Collections.Generic;

namespace MariEngine.Loading;

public class ComponentData
{
    public string ProxyType { get; init; }
    
    public int Priority { get; init; } = 0;
    public bool Enabled { get; init; } = true;

    public bool IsDependency { get; init; } = false;

    public Dictionary<string, string> Resources { get; init; } = new();
    
    public Dictionary<string, string> Specials { get; init; } = new();
}