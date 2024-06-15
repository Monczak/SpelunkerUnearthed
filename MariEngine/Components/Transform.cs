using Microsoft.Xna.Framework;

namespace MariEngine.Components;

public class Transform : Component
{
    public Vector2 Position { get; set; } = Vector2.Zero;
}