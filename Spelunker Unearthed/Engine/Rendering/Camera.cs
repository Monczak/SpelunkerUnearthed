using Microsoft.Xna.Framework;

namespace SpelunkerUnearthed.Engine.Rendering;

public class Camera
{
    private GameWindow window;
    
    public Vector2 Position { get; set; } = Vector2.Zero;
    public float Scale { get; set; } = 1f;
    public int TileSize { get; set; } = 32;

    public Camera(GameWindow window)
    {
        this.window = window;
    }

    public Matrix WorldToScreenMatrix => Matrix.CreateScale(Scale) 
                                         * Matrix.CreateTranslation(-new Vector3(Position.X * TileSize, Position.Y * TileSize, 0))
                                         * Matrix.CreateTranslation(new Vector3(window.ClientBounds.Width / 2f, window.ClientBounds.Height / 2f, 0));

    public Matrix ScreenToWorldMatrix => Matrix.Invert(WorldToScreenMatrix);

    // TODO: This gives wrong Coords
    public Coord ScreenToWorldPoint(Vector2 screenPos)
    {
        Vector3 worldPos = Vector3.Transform(new Vector3(screenPos.X, screenPos.Y, 0), ScreenToWorldMatrix);
        return new Coord((int)worldPos.X, (int)worldPos.Y);
    }
}