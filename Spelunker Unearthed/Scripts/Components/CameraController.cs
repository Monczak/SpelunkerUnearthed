using Microsoft.Xna.Framework;
using MariEngine.Components;
using MariEngine.Rendering;
using MariEngine.Tiles;

namespace SpelunkerUnearthed.Scripts.Components;

public class CameraController : Component
{
    public float Smoothing { get; set; }
    public Vector2 TargetPosition { get; set; }
    
    private Camera camera;
    private TileEntity trackedTileEntity;

    public CameraController(Camera camera)
    {
        this.camera = camera;
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (trackedTileEntity is not null)
            TargetPosition = trackedTileEntity.Tilemap.GetComponent<TilemapRenderer>().CoordToWorldPoint(trackedTileEntity.Position);

        camera.WorldPosition = Vector2.Lerp(camera.WorldPosition, TargetPosition,
            Smoothing * (float)gameTime.ElapsedGameTime.TotalSeconds);
    }

    public void TrackTileEntity(TileEntity entity)
    {
        trackedTileEntity = entity;
    }

    public void UntrackTileEntity()
    {
        trackedTileEntity = null;
    }
}