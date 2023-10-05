using System.Linq;
using MariEngine;
using MariEngine.Components;
using MariEngine.Debugging;
using MariEngine.Input;
using MariEngine.Logging;
using MariEngine.Services;
using MariEngine.Utils;
using Microsoft.Xna.Framework;
using SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;

namespace SpelunkerUnearthed.Scripts.MapGeneration;

public class CaveSystemManager : Component
{
    public CaveSystem CaveSystem { get; } = new();
    
    private Gizmos gizmos;

    public CaveSystemLevel CurrentLevel { get; set; }
    public Room CurrentRoom { get; set; }

    public CaveSystemManager(Gizmos gizmos)
    {
        this.gizmos = gizmos;
        
        // TODO: Remove this, this is for testing only
        // ServiceRegistry.Get<InputManager>().OnPressed("Mine", Generate);
    }

    public void Generate()
    {
        // TODO: Add option for setting the seed
        // ServiceRegistry.Get<RandomNumberGenerator>().Seed(1337);
        CaveSystem.Generate();
    }

    public void DrawLevel(int level)
    {
        foreach (Room room in CaveSystem.Levels[level].Rooms)
        {
            gizmos.DrawRectangle((Vector2)room.Position + Vector2.One * 0.05f, (Vector2)room.Size - Vector2.One * 0.1f,
                Color.Aqua * MathUtils.InverseLerp(20, 0, room.Distance), 0);
            foreach (SubRoomConnection connection in room.Connections)
            {
                gizmos.DrawLine((Vector2)connection.From.Position + Vector2.One * 0.5f, (Vector2)connection.To.Position + Vector2.One * 0.5f, Color.Red, lifetime: 0);
            }
        }
    }

    public void SetCurrentLevel(int level)
    {
        if (level < 0 || level >= CaveSystem.Levels.Count)
            Logger.LogError(
                $"Trying to set cave system level to {level}, but deepest level is {CaveSystem.Levels.Count - 1}");
        else
            CurrentLevel = CaveSystem.Levels[level];
    }

    public void SetCurrentRoom(Room room)
    {
        CurrentRoom = room;
    }

    public void SetCurrentRoomToEntrance()
    {
        SetCurrentRoom(CurrentLevel.EntranceRoom);
    }
    
}