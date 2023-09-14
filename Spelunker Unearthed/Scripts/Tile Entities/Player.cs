using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SpelunkerUnearthed.Engine;
using SpelunkerUnearthed.Engine.Collision;
using SpelunkerUnearthed.Engine.Input;
using SpelunkerUnearthed.Engine.Services;
using SpelunkerUnearthed.Engine.Tiles;

namespace SpelunkerUnearthed.Scripts.TileEntities;

public class Player : TileEntity
{
    private TilemapCollider tilemapCollider;

    private Coord delta = new(0, 0);

    protected override void OnAttach()
    {
        tilemapCollider = Tilemap.GetComponent<TilemapCollider>();

        InputManager inputManager = ServiceRegistry.Get<InputManager>();
        
        // TODO: Switch to moving on key held, as this is annoying
        inputManager.OnPressed("Up", () => delta.Y = -1);
        inputManager.OnPressed("Down", () => delta.Y = 1);
        inputManager.OnPressed("Left", () => delta.X = -1);
        inputManager.OnPressed("Right", () => delta.X = 1);
    }

    public override void Update(GameTime gameTime)
    {
        if (tilemapCollider.TileEntityCollides(this, Position + new Coord(delta.X, 0)))
            delta *= new Coord(0, 1);
        if (tilemapCollider.TileEntityCollides(this, Position + new Coord(0, delta.Y)))
            delta *= new Coord(1, 0);

        if (delta.X * delta.Y != 0 && tilemapCollider.TileEntityCollides(this, Position + delta))
            delta *= new Coord(0, 1);
        
        Move(delta);

        delta = new Coord(0, 0);
    }
}