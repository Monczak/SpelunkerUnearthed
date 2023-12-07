using Microsoft.Xna.Framework;

namespace MariEngine.Components;

public class Transform(Vector2 position) : Component
{
    public Vector2 Position { get; set; } = position;

    public Transform() : this(Vector2.Zero)
    {
    }
}