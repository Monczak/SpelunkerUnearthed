using MariEngine.Input;
using MariEngine.Rendering;
using MariEngine.Services;
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
        gizmoTexture.SetData([Color.White]);
        
        ServiceRegistry.Get<InputManager>().OnPressed("ToggleGizmos", ToggleEnabled);
    }

    private void ToggleEnabled()
    {
        Enabled ^= true;
    }

    protected override void OnAttach()
    {
        gizmos = GetComponent<Gizmos>();
    }

    protected override void Render(SpriteBatch spriteBatch)
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

    protected override void OnDestroy()
    {
        ServiceRegistry.Get<InputManager>().UnbindOnPressed("ToggleGizmos", ToggleEnabled);
    }
}