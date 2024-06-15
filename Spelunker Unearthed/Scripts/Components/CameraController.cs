using System;
using System.Collections.Generic;
using MariEngine;
using Microsoft.Xna.Framework;
using MariEngine.Components;
using MariEngine.Loading;
using MariEngine.Logging;
using MariEngine.Rendering;
using MariEngine.Tiles;
using MariEngine.Utils;

namespace SpelunkerUnearthed.Scripts.Components;

public class CameraController([Inject] Camera camera) : Component<CameraData>
{
    public float Smoothing { get; set; }
    public Vector2 TargetPosition { get; set; }
    public Vector2 CurrentPosition { get; set; }

    private TileEntity trackedTileEntity;

    private SortedDictionary<int, CameraBounds> boundsDict = new();

    public override void Build(CameraData data)
    {
        Smoothing = data.Smoothing;
    }

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (trackedTileEntity is not null)
            TargetPosition = trackedTileEntity.Tilemap.Vector2ToWorldPoint(trackedTileEntity.SmoothedPosition);
        
        foreach (CameraBounds bounds in boundsDict.Values)
            RestrictToBounds(bounds.GetBounds());

        if (Smoothing == 0)
            CurrentPosition = TargetPosition;
        else
            CurrentPosition = Vector2.Lerp(CurrentPosition, TargetPosition,
            Smoothing * (float)gameTime.ElapsedGameTime.TotalSeconds);

        // This manages to fix graphical issues somehow (lines between tiles, tile graphics warping, etc.)
        Vector2 mantissa = CurrentPosition - Vector2.Round(CurrentPosition);
        const float threshold = 0.0001f;
        if (mantissa.X < threshold || mantissa.Y < threshold)
            CurrentPosition += Vector2.One * threshold / 2;
        
        camera.WorldPosition = CurrentPosition;
        

    }

    private void RestrictToBounds(Bounds restrictBounds)
    {
        Bounds viewingWindow = camera.ViewingWindow;

        var (topLeft, bottomRight) = ProcessBounds(restrictBounds, viewingWindow);
        Bounds centerRestrictBounds = Bounds.MakeCorners(topLeft, bottomRight);

        float x = MathUtils.Clamp(TargetPosition.X, centerRestrictBounds.TopLeft.X, centerRestrictBounds.BottomRight.X);
        float y = MathUtils.Clamp(TargetPosition.Y, centerRestrictBounds.TopLeft.Y, centerRestrictBounds.BottomRight.Y);
        TargetPosition = new Vector2(x, y);
    }

    private static (Vector2 topLeft, Vector2 bottomRight) ProcessBounds(Bounds restrictBounds, Bounds viewingWindow)
    {
        Vector2 topLeft = restrictBounds.TopLeft + viewingWindow.Size / 2;
        Vector2 bottomRight = restrictBounds.BottomRight - viewingWindow.Size / 2 + Vector2.One;
        
        if (viewingWindow.Size.X >= restrictBounds.Size.X)
            topLeft.X = bottomRight.X = restrictBounds.TopLeft.X + restrictBounds.Size.X / 2;
        if (viewingWindow.Size.Y >= restrictBounds.Size.Y)
            topLeft.Y = bottomRight.Y = restrictBounds.TopLeft.Y + restrictBounds.Size.Y / 2;

        return (topLeft, bottomRight);
    }

    public void TrackTileEntity(TileEntity entity)
    {
        trackedTileEntity = entity;
    }

    public void UntrackTileEntity()
    {
        trackedTileEntity = null;
    }

    public void SetBounds(int priority, CameraBounds bounds)
    {
        if (bounds is null)
            boundsDict.Remove(priority);
        else
            boundsDict[priority] = bounds;
    }

    public void UnsetBounds()
    {
        boundsDict.Clear();
    }

    public void UnsetBounds(int priority)
    {
        boundsDict.Remove(priority);
    }
}

public class CameraData : ComponentData
{
    public float Smoothing { get; init; }
}