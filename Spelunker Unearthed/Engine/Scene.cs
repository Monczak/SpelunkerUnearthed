using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpelunkerUnearthed.Engine.Rendering;

namespace SpelunkerUnearthed.Engine;

public class Scene
{
    public List<Entity> Entities { get; } = new();

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
        foreach (Entity entity in Entities.Where(e => e.HasComponent<Renderer>()))
        {
            entity.GetComponent<Renderer>().Render(spriteBatch);
        }
    }
}