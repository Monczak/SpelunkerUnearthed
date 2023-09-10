using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpelunkerUnearthed.Engine.Logging;
using SpelunkerUnearthed.Engine.Rendering;

namespace SpelunkerUnearthed.Engine;

public class Scene
{
    public List<Entity> Entities { get; } = new();
    public Camera Camera { get; }

    public Scene(GameWindow window)
    {
        Camera = new Camera(window);
    }    

    public void AddEntity(Entity entity)
    {
        Entities.Add(entity);
    }

    public void Update(GameTime gameTime)
    {
        foreach (Entity entity in Entities)
        {
            entity.Update(gameTime);
        }
    }

    public void Render(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: Camera.WorldToScreenMatrix);
        foreach (Entity entity in Entities.Where(e => e.HasComponent<Renderer>()))
        {
            entity.GetComponent<Renderer>().Render(spriteBatch, Camera);
        }

        spriteBatch.End();
    }
}