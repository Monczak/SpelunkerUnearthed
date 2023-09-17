using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpelunkerUnearthed.Engine.Logging;
using SpelunkerUnearthed.Engine.Rendering;

namespace SpelunkerUnearthed.Engine;

public abstract class Scene
{
    public List<Entity> Entities { get; } = new();
    public Camera Camera { get; }

    protected GraphicsDevice graphicsDevice;

    public abstract void Load();

    public Scene(GameWindow window, GraphicsDevice graphicsDevice)
    {
        Camera = new Camera(window);
        this.graphicsDevice = graphicsDevice;
    }    

    public void AddEntity(Entity entity)
    {
        Entities.Add(entity);
    }

    public virtual void Update(GameTime gameTime)
    {
        foreach (Entity entity in Entities)
        {
            entity.Update(gameTime);
        }

        Entities.RemoveAll(e => e.ToBeDestroyed);
    }

    public void Render(SpriteBatch spriteBatch)
    {
        foreach (Entity entity in Entities.Where(e => e.HasComponent<Renderer>()))
        {
            entity.GetComponent<Renderer>().Render(spriteBatch, Camera);
        }
    }
}