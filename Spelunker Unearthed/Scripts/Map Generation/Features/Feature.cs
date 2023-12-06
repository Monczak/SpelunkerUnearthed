using System.Linq;
using MariEngine;
using MariEngine.Loading;
using MariEngine.Services;
using MariEngine.Tiles;

namespace SpelunkerUnearthed.Scripts.MapGeneration.Features;

public class Feature : Resource<FeatureData>, IFeature
{
    private TileBuffer buffer;
    
    protected override void BuildFromData(FeatureData data)
    {
        Name = data.Name;

        var keys = data.Keys;
        int height = data.Structure.Count;
        int width = data.Structure[0].Length;
        if (data.Structure.Any(row => row.Length != width))
            throw new ResourceLoadingException($"Structure for feature {data.Name} has uneven width.");

        buffer = new TileBuffer(width, height);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var tileChar = data.Structure[y][x];
                buffer[x, y] = tileChar switch
                {
                    ' ' => null,
                    '.' => ServiceRegistry.Get<TileLoader>().Get("Nothing"),
                    _ => ServiceRegistry.Get<TileLoader>().Get(keys[tileChar.ToString()])
                };;
            }
        }

        Bounds = new CoordBounds(Coord.Zero, new Coord(width, height));
    }

    public string Name { get; set; }
    public CoordBounds Bounds { get; set; }
    
    public TileBuffer Generate() => buffer;
}