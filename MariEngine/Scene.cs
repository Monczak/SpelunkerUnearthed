using System;
using System.Collections.Generic;
using System.Linq;
using FmodForFoxes;
using MariEngine.Audio;
using MariEngine.Components;
using MariEngine.Debugging;
using MariEngine.Events;
using MariEngine.Loading;
using MariEngine.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MariEngine.Logging;
using MariEngine.Services;
using YamlDotNet.Serialization;

namespace MariEngine;

public abstract class Scene(GameWindow window, GraphicsDeviceManager graphics) : IProxyBuildable<PackedScene>
{
    public SortedSet<Entity> Entities { get; } = new(new PriorityComparer<Entity>());
    public Camera Camera { get; } = new(window, graphics);

    private readonly PriorityQueue<Renderer, int> rendererQueue = new();

    protected GraphicsDeviceManager graphics = graphics;

    public ComponentFactory ComponentFactory { get; private set; } = new();

    protected Entity GetEntity(string name) => Entities.FirstOrDefault(e => e.Name == name);
    
    public virtual void Load()
    {
        ComponentFactory
            .AddDependency(window)
            .AddDependency(graphics.GraphicsDevice)
            .AddDependency(Camera);
    }

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
        entity.OwnerScene = this;
    }

    public virtual void Update(GameTime gameTime)
    {
        FmodManager.Update();
        
        foreach (var entity in Entities)
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

        while (rendererQueue.TryDequeue(out var renderer, out _))
        {
            renderer.DoRender(spriteBatch);
        }
    }
    
    public void Build(PackedScene data)
    {
        foreach (var (entityName, entityData) in data.SceneData.Entities)
        {
            var entity = new Entity(entityName);
            entity.OwnerScene = this;
            foreach (var (componentTypeName, componentData) in entityData.Components)
            {
                var componentType = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
                    .FirstOrDefault(t => t.IsAssignableTo(typeof(Component)) && t.Name == componentTypeName);
                if (componentType is null)
                    throw new Exception($"{componentTypeName} is not a valid component type.");

                var builder = ComponentFactory.CreateComponentBuilder(componentType);
                if (componentData is not null)
                {
                    if (componentData.GetType() != typeof(ComponentData))
                        builder.WithProxy(componentData.GetType(), componentData);
                   
                    foreach (var (resourceParam, resourceId) in componentData.Resources) 
                        builder.WithResource(resourceParam, resourceId);

                    foreach (var (specialParam, specialValue) in componentData.Specials) 
                        builder.WithSpecial(specialParam, specialValue);
                }

                var component = builder.Build(entity);
                entity.AttachComponent(component);

                if (componentData is not null && componentData.IsDependency)
                    ComponentFactory.AddDependency(componentType, component);
            }
            AddEntity(entity);
        }
    }
}