using Microsoft.Xna.Framework;

namespace MariEngine.Rendering;

public abstract class RendererEffect
{
    public virtual int Priority { get; init; }
    
    public abstract Color Apply(Color input, Coord worldPos);
}