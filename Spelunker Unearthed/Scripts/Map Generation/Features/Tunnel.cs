using System.Collections.Generic;
using MariEngine;
using MariEngine.Services;
using MariEngine.Tiles;
using SpelunkerUnearthed.Scripts.MapGeneration.Utils;

namespace SpelunkerUnearthed.Scripts.MapGeneration.Features;

public class Tunnel(Coord from, Coord to, int stencilSize) : ProceduralFeature
{
    public override CoordBounds Bounds { get; protected set; }
    
    protected override TileBuffer GenerateFeature(TileBuffer map)
    {
        var positions = RandomWalk.WalkTo(from, to, new RandomWalk.Properties
        {
            Variance = 1,
            CanSelfIntersect = false,
            CanGoDiagonally = false,
            DeviationPenalty = 100,
            DistanceCost = 100
        }, pos => map.IsInBounds(pos) ? 1 : 0,
            random: ServiceRegistry.Get<RandomProvider>().Request(Constants.MapGenRng));

        HashSet<Coord> digPositions = [];
        foreach (Coord position in positions)
        {
            // TODO: Rework this to allow for narrower tunnels
            for (int dx = -stencilSize; dx <= stencilSize; dx++)
            {
                for (int dy = -stencilSize; dy <= stencilSize; dy++)
                {
                    digPositions.Add(new Coord(position.X + dx, position.Y + dy));
                }
            }
        }
        
        Bounds = CoordBounds.MakeEnvelope(digPositions);

        TileBuffer buffer = new(Bounds.Size);
        foreach (Coord pos in buffer.Coords)
            buffer[pos] = null;
        
        foreach (Coord pos in digPositions)
            buffer[pos - Bounds.TopLeft] = ServiceRegistry.Get<TileLoader>().Get("Nothing");
        
        return buffer;
    }
}