using MariEngine.Exceptions;

namespace MariEngine.Tiles;

public class TileBufferFragment(TileBuffer buffer, CoordBounds bounds)
{
    public CoordBounds Bounds => bounds;

    public bool IsInBounds(Coord coord) => Bounds.PointInside(coord + Bounds.TopLeft);
    
    public Tile this[Coord coord]
    {
        get
        {
            if (!IsInBounds(coord))
                throw new OutOfBoundsException(coord);
            return buffer[bounds.TopLeft + coord];
        }
        set
        {
            if (!IsInBounds(coord))
                throw new OutOfBoundsException(coord);
            buffer[bounds.TopLeft + coord] = value;
        }
    }

    public Tile this[int x, int y] => this[new Coord(x, y)];
}