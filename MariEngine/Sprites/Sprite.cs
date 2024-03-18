using System;
using System.Linq;
using MariEngine.Loading;
using MariEngine.Services;
using MariEngine.Tiles;

namespace MariEngine.Sprites;

public class Sprite : Resource<SpriteData>
{
    public TileBuffer Tiles { get; private set; }
    public bool NineSliced { get; private set; }
    public int NineSliceCornerSize { get; private set; }

    public Coord Size => new(Tiles.Width, Tiles.Height);

    public Tile GetNineSlice(CoordBounds bounds, Coord pos)
    {
        return (pos - bounds.TopLeft) switch
        {
            // Corners
            var c when c.X < NineSliceCornerSize && c.Y < NineSliceCornerSize => Tiles[c],
            var c when c.X >= bounds.Size.X - NineSliceCornerSize && c.Y < NineSliceCornerSize => Tiles[c.X - bounds.Size.X + Tiles.Width, c.Y],
            var c when c.X < NineSliceCornerSize && c.Y >= bounds.Size.Y - NineSliceCornerSize => Tiles[c.X, c.Y - bounds.Size.Y + Tiles.Height],
            var c when c.X >= bounds.Size.X - NineSliceCornerSize && c.Y >= bounds.Size.Y - NineSliceCornerSize => Tiles[c.X - bounds.Size.X + Tiles.Width, c.Y - bounds.Size.Y + Tiles.Height],
            
            // Horizontal Edges
            var c when c.X >= NineSliceCornerSize && c.Y < NineSliceCornerSize => Tiles[c.X % (Tiles.Width - NineSliceCornerSize * 2), c.Y],
            var c when c.X >= NineSliceCornerSize && c.Y >= bounds.Size.Y - NineSliceCornerSize => Tiles[c.X % (Tiles.Width - NineSliceCornerSize * 2), c.Y - bounds.Size.Y + Tiles.Height],
            
            // Vertical Edges
            var c when c.X < NineSliceCornerSize && c.Y >= NineSliceCornerSize => Tiles[c.X, c.Y % (Tiles.Height - NineSliceCornerSize * 2)],
            var c when c.X >= bounds.Size.X - NineSliceCornerSize && c.Y >= NineSliceCornerSize => Tiles[c.X - bounds.Size.X + Tiles.Width, c.Y % (Tiles.Height - NineSliceCornerSize * 2)],
            
            _ => Tiles[1, 1]    // TODO: Support wrapping inside borders
        };
    }
    
    protected internal override void BuildFromData(SpriteData data)
    {
        var keys = data.Keys;
        int height = data.Tiles.Count;
        int width = data.Tiles[0].Length;
        if (data.Tiles.Any(row => row.Length != width))
            throw new ResourceLoadingException("Sprite has uneven width.");

        Tiles = new TileBuffer(width, height);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var tileChar = data.Tiles[y][x];
                Tiles[x, y] = tileChar switch
                {
                    ' ' => null,
                    '.' => ServiceRegistry.Get<TileLoader>().Get("Nothing"),
                    _ => ServiceRegistry.Get<TileLoader>().Get(keys[tileChar.ToString()])
                };;
            }
        }

        NineSliced = data.NineSliceCornerSize is not null;
        NineSliceCornerSize = data.NineSliceCornerSize ?? 0;

        if (NineSliceCornerSize < 0 || NineSliceCornerSize >= Math.Ceiling(Math.Min(width, height) / 2.0))
            throw new ResourceLoadingException(
                "9-slice corner size must be greater than 0 and less than half of the sprite's width or height, whichever is less.");
    }
}