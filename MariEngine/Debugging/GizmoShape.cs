using MariEngine.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MariEngine.Debugging;

public abstract class GizmoShape
{
    public Vector2 Position { get; init; }
    public Color Color { get; init; }
    
    internal float? Lifetime { get; set; }

    protected GizmoShape(Vector2 position, Color color, float? lifetime = null)
    {
        Position = position;
        Color = color;
        Lifetime = lifetime;
    }

    internal abstract void Render(SpriteBatch spriteBatch, Camera camera, Texture2D texture);
}