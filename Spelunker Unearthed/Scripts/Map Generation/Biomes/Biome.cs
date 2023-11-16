using MariEngine.Loading;
using MariEngine.Utils;
using Microsoft.Xna.Framework;

namespace SpelunkerUnearthed.Scripts.MapGeneration.Biomes;

public class Biome : Resource<BiomeData>
{
    public string Name { get; private set; }
    public Color Color { get; private set; }
    
    protected override void BuildFromData(BiomeData data)
    {
        Name = data.Name;
        Color = ColorUtils.FromHex(data.Color);
    }
}