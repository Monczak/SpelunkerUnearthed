using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MariEngine;
using MariEngine.Collision;
using MariEngine.Components;
using MariEngine.Input;
using MariEngine.Logging;
using MariEngine.Services;
using MariEngine.Tiles;

namespace SpelunkerUnearthed.Scripts.TileEntities;

public class PlayerController : TileEntityComponent
{
    private InputManager inputManager;
    private TilemapCollider tilemapCollider;

    private Coord input = new(0, 0);
    private Coord previousInput;

    private float movementSpeed = 8;
    private Vector2 moveCooldown;

    private Coord facingDirection;

    protected override void OnAttach()
    {
        tilemapCollider = OwnerEntity.Tilemap.GetComponent<TilemapCollider>(); 
        inputManager = ServiceRegistry.Get<InputManager>();

        inputManager.OnPressed("Up", OnUp);
        inputManager.OnPressed("Down", OnDown);
        inputManager.OnPressed("Left", OnLeft);
        inputManager.OnPressed("Right", OnRight);
        
        inputManager.OnReleased("Up", ReadInput);
        inputManager.OnReleased("Down", ReadInput);
        inputManager.OnReleased("Left", ReadInput);
        inputManager.OnReleased("Right", ReadInput);
        
        inputManager.OnPressed("Mine", Mine);
    }

    private void Mine()
    {
        if (OwnerEntity.Tilemap.IsInBounds(OwnerEntity.Position + facingDirection))
            OwnerEntity.Tilemap.Mine(OwnerEntity.Position + facingDirection);
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
            if (tilemapCollider.TileEntityCollides(OwnerEntity, OwnerEntity.Position + delta))
                delta *= Coord.Abs(Coord.Orthogonal(direction));

            OwnerEntity.Move(delta);
            
            if (direction.X == 0)
                moveCooldown.Y = 1f / movementSpeed;
            else
                moveCooldown.X = 1f / movementSpeed;
        }
    }

    public override void Update(GameTime gameTime)
    {
        // TODO: Figure out what to do when mining diagonally
        if (input != Coord.Zero)
            facingDirection = input;
        
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
    
    protected override void OnDestroy()
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