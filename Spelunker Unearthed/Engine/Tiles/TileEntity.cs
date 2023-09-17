using System;
using Microsoft.Xna.Framework;

namespace SpelunkerUnearthed.Engine.Tiles;

public class TileEntity
{
    public Coord Position { get; set; }
    public Tile Tile { get; set; }
    
    public Tilemap Tilemap { get; private set; }

    public void AttachToTilemap(Tilemap tilemap)
    {
        Tilemap = tilemap;
        OnAttach();
    }

    protected virtual void OnAttach()
    {
        
    }

    public virtual void Update(GameTime gameTime)
    {
        
    }

    protected void Move(int dx = 0, int dy = 0)
    {
        Position += new Coord(dx, dy);
        Position = new Coord(MathHelper.Clamp(Position.X, 0, Tilemap.MapWidth - 1),
            MathHelper.Clamp(Position.Y, 0, Tilemap.MapHeight - 1));
    }

    protected void Move(Coord delta) => Move(delta.X, delta.Y);

    public void Destroy()
    {
        Tilemap.TileEntities.Remove(this);
        OnDestroy();
    }

    protected virtual void OnDestroy()
    {
        
    }
}