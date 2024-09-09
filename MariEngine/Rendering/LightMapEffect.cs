using MariEngine.Light;
using Microsoft.Xna.Framework;

namespace MariEngine.Rendering;

public class LightMapEffect(LightMap lightMap) : RendererEffect
{
    public override Color Apply(Color input, Coord worldPos)
    {
        Color light = lightMap.GetRenderedLight(worldPos);
        return new Color(input.R * light.R / 255, input.G * light.G / 255, input.B * light.B / 255, input.A);
    }
}