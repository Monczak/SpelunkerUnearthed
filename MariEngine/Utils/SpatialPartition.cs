using System.Collections.Generic;
using System.Linq;

namespace MariEngine.Utils;

public class SpatialPartition<T>(Coord cellSize) where T : ICoordBoundsProvider
{
    private Dictionary<Coord, HashSet<T>> spatialPartition = new();
    
    private IEnumerable<Coord> GetCellIndices(T boundsProvider, Coord position)
    {
        return GetCellIndices(GetBoundsAtPos(boundsProvider, position));
    }
    
    private IEnumerable<Coord> GetCellIndices(CoordBounds bounds)
    {
        var topLeft = bounds.TopLeft / cellSize;
        var bottomRight = bounds.BottomRight / cellSize;

        for (var y = topLeft.Y; y <= bottomRight.Y; y++)
        {
            for (var x = topLeft.X; x <= bottomRight.X; x++)
            {
                yield return new Coord(x, y);
            }
        }
    }
    
    private CoordBounds GetBoundsAtPos(T boundsProvider, Coord coord)
    {
        var bounds = boundsProvider.GetBounds();
        return new CoordBounds(coord + bounds.TopLeft, bounds.Size);
    }

    public void UpdatePosition(T boundsProvider, Coord oldPos, Coord newPos)
    {
        var oldIndices = GetCellIndices(GetBoundsAtPos(boundsProvider, oldPos));
        var newIndices = GetCellIndices(GetBoundsAtPos(boundsProvider, newPos));

        foreach (var index in oldIndices)
        {
            if (spatialPartition.TryGetValue(index, out var boundsProviders))
            {
                boundsProviders.Remove(boundsProvider);
                if (spatialPartition[index].Count == 0) spatialPartition.Remove(index);
            }
        }

        foreach (var index in newIndices)
        {
            if (!spatialPartition.ContainsKey(index)) spatialPartition.Add(index, []);
            spatialPartition[index].Add(boundsProvider);
        }
    }

    public void Add(T boundsProvider, Coord position)
    {
        foreach (var coord in GetCellIndices(boundsProvider, position))
        {
            spatialPartition.TryAdd(coord, []);
            spatialPartition[coord].Add(boundsProvider);
        }
    }

    public void Remove(T boundsProvider, Coord position)
    {
        foreach (var coord in GetCellIndices(boundsProvider, position))
        {
            spatialPartition[coord]?.Remove(boundsProvider);
            if (spatialPartition[coord].Count == 0) spatialPartition.Remove(coord);
        }

    }

    public IEnumerable<T> Check(T boundsProvider, Coord position)
    {
        var colliderBounds = GetBoundsAtPos(boundsProvider, position);
        var cellIndices = GetCellIndices(colliderBounds);

        return cellIndices.SelectMany(index => spatialPartition.TryGetValue(index, out var value) ? value : []);
    }
}