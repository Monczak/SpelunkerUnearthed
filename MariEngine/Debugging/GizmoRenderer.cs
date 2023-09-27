using MariEngine.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MariEngine.Debugging;

public class GizmoRenderer : Renderer
{
    private Gizmos gizmos;
    
    private readonly Texture2D gizmoTexture;
    
    public GizmoRenderer(GraphicsDevice graphicsDevice, Camera camera) : base(graphicsDevice, camera)
    {
        gizmoTexture = new Texture2D(graphicsDevice, 1, 1, false, SurfaceFormat.Color);
        gizmoTexture.SetData(new[] { Color.White });
    }

    public override void OnAttach()
    {
        gizmos = GetComponent<Gizmos>();
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: camera.TransformMatrix);
        
        foreach (GizmoShape shape in gizmos.Shapes)
        {
            shape.Render(spriteBatch, camera, gizmoTexture);
        }

        spriteBatch.End();
    }

    protected override Vector2 CalculateCenterOffset()
    {
        throw new System.NotImplementedException();
    }
}