using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MariEngine.Components;
using Microsoft.Xna.Framework;

namespace MariEngine;

public class Entity
{
    public string Name { get; set; }

    public bool ToBeDestroyed { get; private set; }

    private Dictionary<Type, Component> components;

    public Entity(string name)
    {
        Name = name;
        components = new Dictionary<Type, Component>();
    }

    public T AddComponent<T>() where T : Component
    {
        AssertComponent<T>();
        
        var component = Activator.CreateInstance<T>();
        AttachComponent(component);
        return (T)components[typeof(T)];
    }

    public T GetComponent<T>() where T : Component
    {
        Type type = components.Keys.FirstOrDefault(t => t.IsAssignableTo(typeof(T)));
        if (type is null) return null;
        return (T)components[type];
    }

    public void RemoveComponent<T>() where T : Component
    {
        T component = GetComponent<T>();
        if (component is null) return;

        components.Remove(typeof(T));
    }

    public void RemoveComponent<T>(T component) where T : Component
    {
        components.Remove(component.GetType());
    }
 
    public void AttachComponent<T>(T component) where T : Component
    {
        AssertComponent<T>();
        
        components[typeof(T)] = component;
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
        if (components.ContainsKey(type))
            throw new Exception($"Entity {Name} already has a component of type {type}");
    }

    private void AssertExclusivity<T>() where T : Component
    {
        Type exclusiveType = components.Keys.FirstOrDefault(t => t.IsAssignableTo(typeof(T)));
        if (typeof(T).IsDefined(typeof(ExclusiveAttribute)) && exclusiveType is not null)
            throw new Exception(
                $"Trying to add component of type {typeof(T).Name} to entity {Name}, but this component is exclusive with {exclusiveType.Name}");
    }

    public void Update(GameTime gameTime)
    {
        foreach (Component component in components.Values)
        {
            component.Update(gameTime);
        }
    }

    public bool HasComponent<T>() where T : Component => components.Keys.Any(c => c.IsAssignableTo(typeof(T)));
    
    public void Destroy()
    {
        ToBeDestroyed = true;
        
        foreach (Component component in components.Values)
            component.Destroy();
    }
}