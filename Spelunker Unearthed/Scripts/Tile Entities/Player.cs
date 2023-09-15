using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SpelunkerUnearthed.Engine;
using SpelunkerUnearthed.Engine.Collision;
using SpelunkerUnearthed.Engine.Input;
using SpelunkerUnearthed.Engine.Logging;
using SpelunkerUnearthed.Engine.Services;
using SpelunkerUnearthed.Engine.Tiles;

namespace SpelunkerUnearthed.Scripts.TileEntities;

public class Player : TileEntity
{
    private InputManager inputManager;
    private TilemapCollider tilemapCollider;

    private Coord input = new(0, 0);
    private Coord previousInput;

    private float movementSpeed = 8;
    private Vector2 moveCooldown;

    protected override void OnAttach()
    {
        tilemapCollider = Tilemap.GetComponent<TilemapCollider>(); 
        inputManager = ServiceRegistry.Get<InputManager>();

        inputManager.OnPressed("Up", OnUp);
        inputManager.OnPressed("Down", OnDown);
        inputManager.OnPressed("Left", OnLeft);
        inputManager.OnPressed("Right", OnRight);
        
        inputManager.OnReleased("Up", ReadInput);
        inputManager.OnReleased("Down", ReadInput);
        inputManager.OnReleased("Left", ReadInput);
        inputManager.OnReleased("Right", ReadInput);
    }

    private void OnUp()
    {
        input.Y = -1;
        ResetCooldowns();
    }

    private void OnDown()
    {
        input.Y = 1;
        ResetCooldowns();
    }

    private void OnLeft()
    {
        input.X = -1;
        ResetCooldowns();
    }

    private void OnRight()
    {
        input.X = 1;
        ResetCooldowns();
    }

    private void MoveDirection(Coord direction)
    {
        Coord delta = input * direction;
        if (moveCooldown * direction == Vector2.Zero && delta != Coord.Zero)
        {
            if (tilemapCollider.TileEntityCollides(this, Position + delta))
                delta *= Coord.Abs(Coord.Orthogonal(direction));

            Move(delta);
            
            if (direction.X == 0)
                moveCooldown.Y = 1f / movementSpeed;
            else
                moveCooldown.X = 1f / movementSpeed;
        }
    }

    public override void Update(GameTime gameTime)
    {
        // TODO: Better movement on diagonals
        MoveDirection(new Coord(1, 0));
        MoveDirection(new Coord(0, 1));

        moveCooldown -= Vector2.One * (float)gameTime.ElapsedGameTime.TotalSeconds;

        moveCooldown.X = moveCooldown.X < 0 ? 0 : moveCooldown.X;
        moveCooldown.Y = moveCooldown.Y < 0 ? 0 : moveCooldown.Y;

        previousInput = input;
    }

    private void ReadInput()
    {
        input = new Coord(0, 0);
        if (inputManager.IsHeld("Up"))
            input.Y = -1;
        else if (inputManager.IsHeld("Down"))
            input.Y = 1;
        
        if (inputManager.IsHeld("Left"))
            input.X = -1;
        else if (inputManager.IsHeld("Right"))
            input.X = 1;
        
        ResetCooldowns();
    }

    private void ResetCooldowns()
    {
        if (previousInput.X != input.X)
            moveCooldown.X = 0;
        if (previousInput.Y != input.Y)
            moveCooldown.Y = 0;
    }

    // TODO: Implement some kind of proper destroy handler instead of overriding destructor (IDisposable?)
    ~Player() 
    {
        inputManager.UnbindOnPressed("Up", OnUp);
        inputManager.UnbindOnPressed("Down", OnDown);
        inputManager.UnbindOnPressed("Left", OnLeft);
        inputManager.UnbindOnPressed("Right", OnRight);
        
        inputManager.UnbindOnReleased("Up", ReadInput);
        inputManager.UnbindOnReleased("Down", ReadInput);
        inputManager.UnbindOnReleased("Left", ReadInput);
        inputManager.UnbindOnReleased("Right", ReadInput);
    }
}