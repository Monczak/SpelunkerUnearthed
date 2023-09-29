using Microsoft.Xna.Framework;

namespace MariEngine.Utils;

public static class Extensions
{
    public static Vector2 Normalized(this Vector2 v) => v / v.Length();
}