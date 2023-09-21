using Microsoft.Xna.Framework;

namespace MariEngine.Components;

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