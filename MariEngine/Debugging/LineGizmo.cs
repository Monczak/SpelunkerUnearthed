using System;
using MariEngine.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MariEngine.Debugging;

public class LineGizmo : GizmoShape
{
    private Vector2 point2;
    private int width;
    
    public LineGizmo(Vector2 point1, Vector2 point2, Color color, int width = 3, float? lifetime = null) : base(point1, color, lifetime)
    {
        this.point2 = point2;
        this.width = width;
    }

    internal override void Render(SpriteBatch spriteBatch, Camera camera, Texture2D texture)
    {
        Rectangle rectangle = new Rectangle((int)(Position.X * camera.TileSize), (int)(Position.Y * camera.TileSize), (int)((point2 - Position).Length() * camera.
            TileSize), width);
        Vector2 v = Vector2.Normalize(Position - point2);
        float angle = MathF.Acos(Vector2.Dot(v, -Vector2.UnitX));
        
        if (Position.Y > point2.Y) 
            angle = MathHelper.TwoPi - angle;
        
        spriteBatch.Draw(texture, rectangle, null, Color, angle, Vector2.Zero, SpriteEffects.None, 0);
    }
}