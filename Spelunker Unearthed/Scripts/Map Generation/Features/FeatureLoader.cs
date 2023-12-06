using MariEngine.Services;

namespace SpelunkerUnearthed.Scripts.MapGeneration.Features;

public class FeatureLoader : ResourceLoaderService<Feature, FeatureData>
{
    protected override string ContentPath => ContentPaths.Features;
}