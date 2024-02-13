using MariEngine.Tiles;

namespace SpelunkerUnearthed.Scripts.MapGeneration.Features;

public static class FeaturePlacer
{
    public static void Place(IFeature feature, TileBuffer map, int layerId)
    {
        map.PasteAt(feature.Generate(map), feature.Bounds.TopLeft);
    }
}