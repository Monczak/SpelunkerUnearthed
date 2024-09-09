using MariEngine.Input;
using MariEngine.Loading;
using MariEngine.Rendering;
using MariEngine.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MariEngine.Debugging;

public class GizmoRenderer : Renderer
{
    private Gizmos gizmos;
    
    private readonly Texture2D gizmoTexture;
    
    public GizmoRenderer([Inject] GraphicsDevice graphicsDevice, [Inject] Camera camera) : base(graphicsDevice, camera)
    {
        gizmoTexture = new Texture2D(graphicsDevice, 1, 1, false, SurfaceFormat.Color);
        gizmoTexture.SetData([Color.White]);
        
        ServiceRegistry.Get<InputManager>().OnPressed(this, "ToggleGizmos", ToggleEnabled);
    }

    private void ToggleEnabled()
    {
        Enabled ^= true;
    }

    protected internal override void Initialize()
    {
        gizmos = GetComponent<Gizmos>();
    }

    protected override void Render(SpriteBatch spriteBatch, GameTime gameTime)
    {
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: Camera.TransformMatrix);
        
        foreach (GizmoShape shape in gizmos.Shapes)
        {
            shape.Render(spriteBatch, Camera, gizmoTexture);
        }

        spriteBatch.End();
    }

    protected override Vector2 CalculateCenterOffset()
    {
        throw new System.NotImplementedException();
    }

    protected override void OnDestroy()
    {
        ServiceRegistry.Get<InputManager>().UnbindOnPressed(this, "ToggleGizmos", ToggleEnabled);
    }
}