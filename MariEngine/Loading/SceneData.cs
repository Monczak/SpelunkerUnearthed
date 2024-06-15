using System.Collections.Generic;

namespace MariEngine.Loading;

public record SceneData
{
    public Dictionary<string, EntityData> Entities { get; init; }
}