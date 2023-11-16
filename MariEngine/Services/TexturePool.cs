using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace MariEngine.Services;

public class TexturePool : Service
{
    private Dictionary<Guid, Texture2D> textures = new();
    private GraphicsDevice graphicsDevice;

    public TexturePool(GraphicsDevice graphicsDevice)
    {
        this.graphicsDevice = graphicsDevice;
    }

    public Texture2D RequestTexture(Coord size, out Guid guid)
    {
        guid = Guid.NewGuid();
        textures.TryAdd(guid, new Texture2D(graphicsDevice, size.X, size.Y));
        return textures[guid];
    }

    public Texture2D RequestTexture(Guid guid)
    {
        return textures.TryGetValue(guid, out var texture) ? texture : null;
    }
}