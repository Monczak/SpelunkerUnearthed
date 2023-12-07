using MariEngine.Tiles;

namespace SpelunkerUnearthed.Scripts.MapGeneration.Features;

public static class FeaturePlacer
{
    public static void Place(IFeature feature, Tilemap tilemap, int layerId)
    {
        tilemap.PasteAt(feature.Generate(tilemap), feature.Bounds.TopLeft, layerId);
    }
}