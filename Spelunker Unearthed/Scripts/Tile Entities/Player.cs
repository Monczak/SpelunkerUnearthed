using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SpelunkerUnearthed.Engine;
using SpelunkerUnearthed.Engine.Collision;
using SpelunkerUnearthed.Engine.Tiles;

namespace SpelunkerUnearthed.Scripts.TileEntities;

public class Player : TileEntity
{
    private TilemapCollider tilemapCollider;

    protected override void OnAttach()
    {
        tilemapCollider = Tilemap.GetComponent<TilemapCollider>();
    }

    public override void Update(GameTime gameTime)
    {
        Coord delta = new(0, 0);
        
        var state = Keyboard.GetState();
        if (state.IsKeyDown(Keys.Right))
            delta.X = 1;
        if (state.IsKeyDown(Keys.Left))
            delta.X = -1;
        if (state.IsKeyDown(Keys.Up))
            delta.Y = -1;
        if (state.IsKeyDown(Keys.Down))
            delta.Y = 1;
        
        if (tilemapCollider.TileEntityCollides(this, Position + new Coord(delta.X, 0)))
            delta *= new Coord(0, 1);
        if (tilemapCollider.TileEntityCollides(this, Position + new Coord(0, delta.Y)))
            delta *= new Coord(1, 0);

        if (delta.X * delta.Y != 0 && tilemapCollider.TileEntityCollides(this, Position + delta))
            delta *= new Coord(0, 1);
        
        Move(delta);
    }
}