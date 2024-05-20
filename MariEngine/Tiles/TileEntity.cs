using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MariEngine.Components;
using MariEngine.Utils;
using Microsoft.Xna.Framework;

namespace MariEngine.Tiles;

// TODO: Refactor this to use a generic base class for a component container
public class TileEntity(string name) : IAudioListener
{
    public string Name { get; set; } = name;

    private Coord position;

    public Coord Position
    {
        get => position;
        set
        {
            var oldPosition = position;
            position = value;
            
            foreach (TileEntityComponent component in components.Values)
                component.OnPositionUpdate();
            
            PositionUpdated?.Invoke(this, oldPosition, position);
            
            Tilemap.StepOn(this, position);
        }
    }
    
    public Vector2 SmoothedPosition { get; private set; }
    public float PositionSmoothing { get; set; }

    public Tilemap Tilemap { get; private set; }

    private Dictionary<Type, TileEntityComponent> components = new();

    public delegate void PositionUpdatedHandler(TileEntity sender, Coord oldPos, Coord newPos);
    public event PositionUpdatedHandler PositionUpdated;

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


    internal void AttachToTilemap(Tilemap tilemap)
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

        if (PositionSmoothing == 0)
            SmoothedPosition = (Vector2)Position;
        else
            SmoothedPosition = Vector2.Lerp(SmoothedPosition, (Vector2)Position,
            PositionSmoothing * (float)gameTime.ElapsedGameTime.TotalSeconds);
    }

    public void Move(int dx = 0, int dy = 0)
    {
        Position += new Coord(dx, dy);
        Position = new Coord(MathHelper.Clamp(Position.X, 0, Tilemap.Width - 1),
            MathHelper.Clamp(Position.Y, 0, Tilemap.Height - 1));
    }

    public void Move(Coord delta) => Move(delta.X, delta.Y);

    public void Destroy()
    {
        Tilemap.RemoveTileEntity(this);
        OnDestroy();
    }

    protected virtual void OnDestroy()
    {
        foreach (var (type, component) in components)
            component.Destroy();
    }

    public Vector2 GetPosition() => Tilemap.CoordToWorldPoint(Position);
}