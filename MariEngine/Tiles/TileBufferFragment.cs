using MariEngine.Exceptions;

namespace MariEngine.Tiles;

public class TileBufferFragment(TileBuffer buffer, CoordBounds bounds)
{
    private TileBuffer Buffer => buffer;
    
    public CoordBounds Bounds => bounds;

    public bool IsInBounds(Coord coord) => Bounds.PointInside(coord + Bounds.TopLeft);

    public Tile GetRelative(Coord coord) => this[coord];
    public void SetRelative(Coord coord, Tile tile) => this[coord] = tile;

    public Tile GetAbsolute(Coord coord) => this[coord - bounds.TopLeft];
    public Tile SetAbsolute(Coord coord, Tile tile) => this[coord - bounds.TopLeft] = tile;

    public TileBufferFragment(TileBufferFragment fragment, CoordBounds internalBounds) 
        : this(fragment.Buffer, new CoordBounds(internalBounds.TopLeft + fragment.Bounds.TopLeft, internalBounds.Size))
    {
        
    }
    
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