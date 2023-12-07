using MariEngine.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MariEngine.Debugging;

public abstract class GizmoShape(Vector2 position, Color color, float? lifetime = null)
{
    public Vector2 Position { get; init; } = position;
    public Color Color { get; init; } = color;

    internal float? Lifetime { get; set; } = lifetime;

    internal abstract void Render(SpriteBatch spriteBatch, Camera camera, Texture2D texture);
}