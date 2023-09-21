using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpelunkerUnearthed.Engine.Rendering;

public class Camera
{
    private GameWindow window;
    private GraphicsDeviceManager graphics;
    
    public Vector2 Position { get; set; } = Vector2.Zero;
    public float Scale { get; set; } = 1f;
    public int TileSize { get; set; } = 16;

    public Vector2 WorldPosition
    {
        get => Position / TileSize;
        set => Position = value * TileSize;
    }

    public Camera(GameWindow window, GraphicsDeviceManager graphics)
    {
        this.window = window;
        this.graphics = graphics;
    }

    public Matrix TransformMatrix => Matrix.CreateScale(Scale)
                                         * Matrix.CreateTranslation(-new Vector3(Position.X, Position.Y, 0))
                                         * Matrix.CreateTranslation(new Vector3(window.ClientBounds.Width / 2f, window.ClientBounds.Height / 2f, 0));

    public Matrix InverseTransformMatrix => Matrix.Invert(TransformMatrix);
    
    public Coord ScreenToWorldPoint(Vector2 screenPos)
    {
        Vector3 worldPos = Vector3.Transform(new Vector3(screenPos, 0), InverseTransformMatrix) / TileSize;
        return new Coord((int)MathF.Floor(worldPos.X), (int)MathF.Floor(worldPos.Y));
    }
}