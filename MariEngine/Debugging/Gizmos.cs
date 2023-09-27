using System.Collections.Generic;
using MariEngine.Components;
using Microsoft.Xna.Framework;

namespace MariEngine.Debugging;

public class Gizmos : Component
{
    internal List<GizmoShape> Shapes { get; private set; } = new();
    
    public void DrawRectangle(Vector2 position, Vector2 size, Color color, float? lifetime = null)
    {
        Shapes.Add(new RectangleGizmo(position, size, color, lifetime));
    }

    public void DrawLine(Vector2 begin, Vector2 end, Color color, int width = 3, float? lifetime = null)
    {
        Shapes.Add(new LineGizmo(begin, end, color, width, lifetime));
    }

    public override void Update(GameTime gameTime)
    {
        foreach (var shape in Shapes)
        {
            if (shape.Lifetime is not null)
                shape.Lifetime -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        Shapes.RemoveAll(shape => shape.Lifetime is not null && shape.Lifetime < 0);
    }
}