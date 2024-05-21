using System.Collections.Generic;
using MariEngine.Components;
using MariEngine.Input;
using MariEngine.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MariEngine.Debugging;

public class Gizmos : Component
{
    public static Gizmos Default { get; private set; }
    
    private object lockObj = new();

    public static void SetDefault(Gizmos gizmos) => Default = gizmos;
    
    internal List<GizmoShape> Shapes { get; private set; } = [];
    
    public void DrawRectangle(Vector2 position, Vector2 size, Color color, float? lifetime = null)
    {
        lock (lockObj) Shapes.Add(new RectangleGizmo(position, size, color, lifetime));
    }

    public void DrawLine(Vector2 begin, Vector2 end, Color color, int width = 3, float? lifetime = null)
    {
        lock (lockObj) Shapes.Add(new LineGizmo(begin, end, color, width, lifetime));
    }

    public void DrawTexture(Vector2 position, Vector2 size, Color color, Texture2D texture, float? lifetime = null)
    {
        lock (lockObj) Shapes.Add(new TextureGizmo(position, size, color, texture, lifetime));
    }
    
    protected override void Update(GameTime gameTime)
    {
        lock (lockObj)
        {
            foreach (var shape in Shapes)
            {
                if (shape.Lifetime is not null)
                    shape.Lifetime -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            Shapes.RemoveAll(shape => shape.Lifetime is not null && shape.Lifetime < 0);
        }
    }

    protected override void OnDestroy()
    {
        Shapes.Clear();
        base.OnDestroy();
    }
}