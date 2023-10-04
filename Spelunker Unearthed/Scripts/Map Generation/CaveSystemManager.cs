using MariEngine.Components;
using MariEngine.Debugging;
using MariEngine.Input;
using MariEngine.Services;
using MariEngine.Utils;
using Microsoft.Xna.Framework;
using SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;

namespace SpelunkerUnearthed.Scripts.MapGeneration;

public class CaveSystemManager : Component
{
    private CaveSystem caveSystem = new();
    
    private Gizmos gizmos;

    public CaveSystemManager(Gizmos gizmos)
    {
        this.gizmos = gizmos;
        
        // TODO: Remove this, this is for testing only
        ServiceRegistry.Get<InputManager>().OnPressed("Mine", Generate);
    }

    public void Generate()
    {
        // ServiceRegistry.Get<RandomNumberGenerator>().Seed(1337);
        caveSystem.Generate();
    }

    public void DrawLevel(int level)
    {
        foreach (Room room in caveSystem.Levels[level].Rooms)
        {
            gizmos.DrawRectangle((Vector2)room.Position + Vector2.One * 0.05f, (Vector2)room.Size - Vector2.One * 0.1f,
                Color.Aqua * MathUtils.InverseLerp(20, 0, room.Distance), 0);
            foreach (SubRoomConnection connection in room.Connections)
            {
                gizmos.DrawLine((Vector2)connection.From.Position + Vector2.One * 0.5f, (Vector2)connection.To.Position + Vector2.One * 0.5f, Color.Red, lifetime: 0);
            }
        }
    }
}