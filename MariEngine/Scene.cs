using System.Collections.Generic;
using System.Linq;
using FmodForFoxes;
using MariEngine.Audio;
using MariEngine.Components;
using MariEngine.Debugging;
using MariEngine.Events;
using MariEngine.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MariEngine.Logging;
using MariEngine.Services;

namespace MariEngine;

public abstract class Scene(GameWindow window, GraphicsDeviceManager graphics)
{
    public SortedSet<Entity> Entities { get; } = new(new PriorityComparer<Entity>());
    public Camera Camera { get; } = new(window, graphics);

    private readonly PriorityQueue<Renderer, int> rendererQueue = new();

    protected GraphicsDeviceManager graphics = graphics;

    public abstract void Load();

    public virtual void Unload()
    {
        foreach (var entity in Entities)
            entity.Destroy();
        
        ServiceRegistry.Get<DebugScreen>().RemoveAllLines(this);
        ServiceRegistry.Get<EventManager>().UnbindAll(this);
        ServiceRegistry.Get<AudioManager>().UnloadAllBanks(this);
    }

    internal void InitializeEntities()
    {
        foreach (var entity in Entities)
        {
            entity.InitializeComponents();
        }
        Initialize();
    }

    protected virtual void Initialize()
    {
        
    }
    
    public void AddEntity(Entity entity)
    {
        Entities.Add(entity);
    }

    public virtual void Update(GameTime gameTime)
    {
        FmodManager.Update();
        
        foreach (Entity entity in Entities)
        {
            entity.Update(gameTime);
        }

        Entities.RemoveWhere(e => e.ToBeDestroyed);
    }

    public void Render(SpriteBatch spriteBatch)
    {
        // TODO: Check if this needs to be optimized for memory / GC
        rendererQueue.Clear(); 
        rendererQueue.EnqueueRange(Entities
            .Select(entity => entity.GetComponent<Renderer>())
            .Where(renderer => renderer is not null)
            .Select(renderer => (renderer, renderer.Layer)));

        while (rendererQueue.TryDequeue(out Renderer renderer, out _))
        {
            renderer.DoRender(spriteBatch);
        }
    }
}