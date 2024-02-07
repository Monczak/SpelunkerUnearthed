using MariEngine.Sprites;

namespace MariEngine.Services;

public class SpriteLoader : ResourceLoaderService<Sprite, SpriteData>
{
    protected override string ContentPath => ContentPaths.Sprites;
}