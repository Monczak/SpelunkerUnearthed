using System.Collections.Generic;
using MariEngine;
using MariEngine.Services;
using MariEngine.Utils;

namespace SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;

public abstract class RoomDecisionEngine
{
    protected abstract Coord MinRoomSize { get; }
    protected abstract Coord MaxRoomSize { get; }
    
    protected abstract float GetSizeWeight(Room sourceRoom, Coord newRoomSize);
    
    public abstract float GetPlacementWeight(Room sourceRoom, (Coord pos, AttachNode node) placement, Coord newRoomSize, List<Room> allRooms);
    public abstract float GetBranchingProbability(Room sourceRoom);
    public abstract float GetContinueProbability(Room newRoom);

    public abstract float GetNeighborConnectionProbability(Room sourceRoom, Room neighborRoom);

    public abstract bool ShouldRegenerate(CaveSystemLevel level);

    public Coord PickRoomSize(Room sourceRoom)
    {
        List<(Coord size, float weight)> sizes = new();
        for (int y = MinRoomSize.Y; y <= MaxRoomSize.Y; y++)
        {
            for (int x = MinRoomSize.X; x <= MaxRoomSize.X; x++)
            {
                Coord size = new(x, y);
                sizes.Add((size, GetSizeWeight(sourceRoom, size)));
            }
        }

        Random random = ServiceRegistry.Get<RandomProvider>().Request(Constants.CaveSystemGen);
        return random.PickWeighted(sizes, out _);
    }
}