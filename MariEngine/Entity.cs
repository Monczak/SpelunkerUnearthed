using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MariEngine.Components;
using Microsoft.Xna.Framework;

namespace MariEngine;

public class Entity(string name) : IPriorityItem
{
    public Scene OwnerScene { get; internal set; }
    
    public string Name { get; set; } = name;
    
    public virtual int Priority { get; init; } = 0;

    public bool ToBeDestroyed { get; private set; }
    
    private readonly SortedSet<Component> components = new(new PriorityComparer<Component>());

    public T AddComponent<T>() where T : Component
    {
        AssertComponent<T>();
        
        var component = Activator.CreateInstance<T>();
        AttachComponent(component);
        return component;
    }

    public T GetComponent<T>() where T : Component
    {
        var component = components.FirstOrDefault(c => c.GetType().IsAssignableTo(typeof(T)));
        return (T)component;
    }

    public void RemoveComponent<T>() where T : Component
    {
        var component = GetComponent<T>();
        if (component is null) return;

        components.Remove(component);
    }

    public void RemoveComponent<T>(T component) where T : Component
    {
        components.Remove(component);
    }
 
    public void AttachComponent<T>(T component) where T : Component
    {
        AssertComponent<T>();
        
        components.Add(component);
        component.SetOwner(this);
    }

    private void AssertComponent<T>() where T : Component
    {
        AssertUniqueComponentType<T>();
        AssertExclusivity<T>();
    }
    
    private void AssertUniqueComponentType<T>() where T : Component
    {
        var type = typeof(T);
        if (components.Any(c => c.GetType() == type))
            throw new Exception($"Entity {Name} already has a component of type {type}");
    }

    private void AssertExclusivity<T>() where T : Component
    {
        var exclusiveComponent = components.FirstOrDefault(c => c.GetType().IsAssignableTo(typeof(T)));
        if (typeof(T).IsDefined(typeof(ExclusiveAttribute)) && exclusiveComponent is not null)
            throw new Exception(
                $"Trying to add component of type {typeof(T).Name} to entity {Name}, but this component is exclusive with {exclusiveComponent.GetType().Name}");
    }

    internal void InitializeComponents()
    {
        foreach (var component in components)
        {
            component.Initialize();
        }
    }

    public void Update(GameTime gameTime)
    {
        foreach (var component in components)
        {
            component.DoUpdate(gameTime);
        }
    }

    public bool HasComponent<T>() where T : Component => components.Any(c => c.GetType().IsAssignableTo(typeof(T)));
    
    public void Destroy()
    {
        ToBeDestroyed = true;

        foreach (var component in components) 
            component.DestroyWithoutRemove();
        components.Clear();
    }
}