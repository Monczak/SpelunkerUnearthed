using Microsoft.Xna.Framework;

namespace SpelunkerUnearthed.Engine.Components;

public class Transform : Component
{
    public Vector2 Position { get; set; }

    public Transform()
    {
        Position = Vector2.Zero;
    }

    public Transform(Vector2 position)
    {
        Position = position;
    }
}