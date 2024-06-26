﻿using System;
using MariEngine.Services;
using MariEngine.Tiles;
using MariEngine.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MariEngine.Rendering;

public class Camera(GameWindow window, GraphicsDeviceManager graphics) : IAudioListener
{
    private GraphicsDeviceManager graphics = graphics;
    private TileAtlas tileAtlas = ServiceRegistry.Get<TileAtlas>();
    
    public Vector2 Position { get; set; } = Vector2.Zero;
    public float Scale { get; set; } = 1f;
    public int TileSize => tileAtlas.TileSize;

    public Vector2 WorldPosition
    {
        get => Position / TileSize;
        set => Position = value * TileSize;
    }

    public Matrix TransformMatrix => Matrix.CreateScale(Scale)
                                     * Matrix.CreateTranslation(-new Vector3(Position.X, Position.Y, 0))
                                     * Matrix.CreateTranslation(new Vector3(window.ClientBounds.Width / 2f, window.ClientBounds.Height / 2f, 0));

    public Matrix InverseTransformMatrix => Matrix.Invert(TransformMatrix);
    
    public Vector2 ScreenToWorldPoint(Vector2 screenPos)
    {
        Vector3 worldPos = Vector3.Transform(new Vector3(screenPos, 0), InverseTransformMatrix) / TileSize;
        return new Vector2(worldPos.X, worldPos.Y);
    }

    public Bounds ViewingWindow => Bounds.MakeCorners(ScreenToWorldPoint(Vector2.Zero),
        ScreenToWorldPoint(new Vector2(window.ClientBounds.Width, window.ClientBounds.Height)));

    public Vector2 GetPosition() => WorldPosition;
}