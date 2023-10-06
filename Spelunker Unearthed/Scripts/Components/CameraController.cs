using System;
using MariEngine;
using Microsoft.Xna.Framework;
using MariEngine.Components;
using MariEngine.Logging;
using MariEngine.Rendering;
using MariEngine.Tiles;
using MariEngine.Utils;

namespace SpelunkerUnearthed.Scripts.Components;

public class CameraController : Component
{
    public float Smoothing { get; set; }
    public Vector2 TargetPosition { get; set; }
    
    private Camera camera;
    private TileEntity trackedTileEntity;

    private CameraBounds bounds;

    public CameraController(Camera camera)
    {
        this.camera = camera;
    }

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (trackedTileEntity is not null)
            TargetPosition = trackedTileEntity.Tilemap.GetComponent<TilemapRenderer>().CoordToWorldPoint(trackedTileEntity.Position);
        
        if (bounds is not null)
            RestrictToBounds();

        camera.WorldPosition = Vector2.Lerp(camera.WorldPosition, TargetPosition + Vector2.One * 0.5f,
            Smoothing * (float)gameTime.ElapsedGameTime.TotalSeconds);
    }

    private void RestrictToBounds()
    {
        Bounds viewingWindow = camera.ViewingWindow;
        Bounds restrictBounds = bounds.GetBounds();

        Vector2 topLeft = restrictBounds.TopLeft + viewingWindow.Size / 2;
        Vector2 bottomRight = restrictBounds.BottomRight - viewingWindow.Size / 2 + Vector2.One;
        if (viewingWindow.Size.X >= restrictBounds.Size.X)
        {
            topLeft.X = bottomRight.X = restrictBounds.TopLeft.X + restrictBounds.Size.X / 2;
        }
        if (viewingWindow.Size.Y >= restrictBounds.Size.Y)
        {
            topLeft.Y = bottomRight.Y = restrictBounds.TopLeft.Y + restrictBounds.Size.Y / 2;
        }
        
        Bounds centerRestrictBounds = Bounds.MakeCorners(topLeft, bottomRight);

        float x = MathUtils.Clamp(TargetPosition.X, centerRestrictBounds.TopLeft.X, centerRestrictBounds.BottomRight.X);
        float y = MathUtils.Clamp(TargetPosition.Y, centerRestrictBounds.TopLeft.Y, centerRestrictBounds.BottomRight.Y);
        TargetPosition = new Vector2(x, y);
    }

    public void TrackTileEntity(TileEntity entity)
    {
        trackedTileEntity = entity;
    }

    public void UntrackTileEntity()
    {
        trackedTileEntity = null;
    }

    public void SetBounds(CameraBounds bounds)
    {
        this.bounds = bounds;
    }

    public void UnsetBounds()
    {
        bounds = null;
    }
}