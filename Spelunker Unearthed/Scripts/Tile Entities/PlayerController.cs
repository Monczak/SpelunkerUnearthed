using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MariEngine;
using MariEngine.Audio;
using MariEngine.Collision;
using MariEngine.Components;
using MariEngine.Debugging;
using MariEngine.Events;
using MariEngine.Input;
using MariEngine.Light;
using MariEngine.Logging;
using MariEngine.Rendering;
using MariEngine.Services;
using MariEngine.Tiles;
using MariEngine.Utils;
using SpelunkerUnearthed.Scripts.MapGeneration.Biomes;
using SpelunkerUnearthed.Scripts.Scenes;

namespace SpelunkerUnearthed.Scripts.TileEntities;

public class PlayerController : TileEntityComponent
{
    private InputManager inputManager;
    private TilemapCollider tilemapCollider;

    private Coord input = new(0, 0);
    private Coord previousInput;

    private float movementSpeed = 8;
    private Vector2 moveCooldown;

    public Coord FacingDirection { get; private set; }

    private DebugScreenLine<(Coord, Vector2)> playerPosDebugLine;
    private DebugScreenLine<Biome> biomeDebugLine;
    
    protected override void Initialize()
    {
        tilemapCollider = OwnerEntity.Tilemap.GetComponent<TilemapCollider>(); 
        inputManager = ServiceRegistry.Get<InputManager>();

        using (var context = inputManager.CreateContext(this))
        {
            context.OnPressed("Up", OnUp);
            context.OnPressed("Down", OnDown);
            context.OnPressed("Left", OnLeft);
            context.OnPressed("Right", OnRight);
        
            context.OnReleased("Up", ReadInput);
            context.OnReleased("Down", ReadInput);
            context.OnReleased("Left", ReadInput);
            context.OnReleased("Right", ReadInput);
        
            context.OnPressed("Mine", Mine);
            context.OnPressed("Use", Use);
        }

        playerPosDebugLine = new DebugScreenLine<(Coord, Vector2)>(tuple => $"Position: {tuple.Item1} World: ({tuple.Item2.X}, {tuple.Item2.Y})");
        ServiceRegistry.Get<DebugScreen>().AddLine(this, playerPosDebugLine);
    }

    private void Use()
    {
        // TODO: DEBUG - remove this
        // ServiceRegistry.Get<TileAtlas>().Resize(ServiceRegistry.Get<TileAtlas>().TileSize + 1);
        ServiceRegistry.Get<SceneManager>().LoadScene<EmptyScene>();
    }

    private void Mine()
    {
        var tilePos = OwnerEntity.Position + FacingDirection;
        if (OwnerEntity.Tilemap.IsInBounds(tilePos))
        {
            OwnerEntity.Tilemap.Mine(tilePos, TilemapLayer.Base);
        }
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
            FacingDirection = input;
        
        // TODO: Better movement on diagonals
        MoveDirection(new Coord(1, 0));
        MoveDirection(new Coord(0, 1));

        moveCooldown -= Vector2.One * (float)gameTime.ElapsedGameTime.TotalSeconds;

        moveCooldown.X = moveCooldown.X < 0 ? 0 : moveCooldown.X;
        moveCooldown.Y = moveCooldown.Y < 0 ? 0 : moveCooldown.Y;

        previousInput = input;
        
        playerPosDebugLine.SetParams((OwnerEntity.Position, OwnerEntity.Tilemap.CoordToWorldPoint(OwnerEntity.Position)));
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
        ServiceRegistry.Get<DebugScreen>().RemoveAllLines(this);
        inputManager.UnbindAll(this);
    }

    protected override void OnPositionUpdate()
    {
        base.OnPositionUpdate();
        ServiceRegistry.Get<EventManager>().Notify("PlayerMoved", this);
    }
}