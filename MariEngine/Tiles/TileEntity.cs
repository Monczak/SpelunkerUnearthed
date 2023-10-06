using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MariEngine.Components;
using Microsoft.Xna.Framework;

namespace MariEngine.Tiles;

// TODO: Refactor this to use a generic base class for a component container
public class TileEntity
{
    public string Name { get; set; }

    private Coord position;

    public Coord Position
    {
        get => position;
        set
        {
            position = value;
            foreach (TileEntityComponent component in components.Values)
                component.OnPositionUpdate();
            Tilemap.StepOn(this, position);
        }
    }
    public Tile Tile { get; set; }
    
    public Tilemap Tilemap { get; private set; }

    private Dictionary<Type, TileEntityComponent> components;

    public TileEntity(string name)
    {
        Name = name;
        components = new Dictionary<Type, TileEntityComponent>();
    }
    
    public T AddComponent<T>() where T : TileEntityComponent
    {
        AssertComponent<T>();
        
        var component = Activator.CreateInstance<T>();
        AttachComponent(component);
        return (T)components[typeof(T)];
    }

    public T GetComponent<T>() where T : TileEntityComponent
    {
        Type type = null;
        foreach (var t in components.Keys)
        {
            if (t.IsAssignableTo(typeof(T)))
            {
                type = t;
                break;
            }
        }

        if (type is null) return null;
        return (T)components[type];
    }

    public void RemoveComponent<T>() where T : TileEntityComponent
    {
        T component = GetComponent<T>();
        if (component is null) return;

        components.Remove(typeof(T));
    }

    public void RemoveComponent<T>(T component) where T : TileEntityComponent
    {
        components.Remove(component.GetType());
    }
 
    public void AttachComponent<T>(T component) where T : TileEntityComponent
    {
        AssertComponent<T>();
        
        components[typeof(T)] = component;
        component.SetOwner(this);
    }

    private void AssertComponent<T>() where T : TileEntityComponent
    {
        AssertUniqueComponentType<T>();
        AssertExclusivity<T>();
    }
    
    private void AssertUniqueComponentType<T>() where T : TileEntityComponent
    {
        var type = typeof(T);
        if (components.ContainsKey(type))
            throw new Exception($"Tile entity {Name} already has a component of type {type}");
    }

    private void AssertExclusivity<T>() where T : TileEntityComponent
    {
        Type exclusiveType = components.Keys.FirstOrDefault(t => t.IsAssignableTo(typeof(T)));
        if (typeof(T).IsDefined(typeof(ExclusiveAttribute)) && exclusiveType is not null)
            throw new Exception(
                $"Trying to add tile entity component of type {typeof(T).Name} to entity {Name}, but this component is exclusive with {exclusiveType.Name}");
    }
    
    public bool HasComponent<T>() where T : Component => components.Keys.Any(c => c.IsAssignableTo(typeof(T)));


    public void AttachToTilemap(Tilemap tilemap)
    {
        Tilemap = tilemap;
        OnAttachToTilemap();
    }

    public virtual void OnAttachToTilemap()
    {
        
    }

    public void Update(GameTime gameTime)
    {
        foreach (TileEntityComponent component in components.Values)
            component.Update(gameTime);
    }

    public void Move(int dx = 0, int dy = 0)
    {
        Position += new Coord(dx, dy);
        Position = new Coord(MathHelper.Clamp(Position.X, 0, Tilemap.MapWidth - 1),
            MathHelper.Clamp(Position.Y, 0, Tilemap.MapHeight - 1));
    }

    public void Move(Coord delta) => Move(delta.X, delta.Y);

    public void Destroy()
    {
        Tilemap.TileEntities.Remove(this);
        OnDestroy();
    }

    protected virtual void OnDestroy()
    {
        
    }
}