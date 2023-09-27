using MariEngine.Logging;
using MariEngine.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MariEngine.Debugging;

public class RectangleGizmo : GizmoShape
{
    public Vector2 Size { get; init; }
    
    internal override void Render(SpriteBatch spriteBatch, Camera camera, Texture2D texture)
    {
        Point position = ((Position - Size / 2) * camera.TileSize).ToPoint();
        Point size = (Size * camera.TileSize).ToPoint();
        
        spriteBatch.Draw(texture, new Rectangle(position, size), Color);
    }

    public RectangleGizmo(Vector2 position, Vector2 size, Color color, float? lifetime = null) : base(position, color, lifetime)
    {
        Size = size;
    }
}