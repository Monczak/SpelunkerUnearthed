using System.Collections.Generic;

namespace MariEngine.Sprites;

public record struct SpriteData(List<string> Tiles, Dictionary<string, string> Keys, int? NineSliceCornerSize);