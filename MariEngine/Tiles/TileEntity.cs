using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MariEngine.Components;
using MariEngine.Services;
using MariEngine.Utils;
using Microsoft.Xna.Framework;

namespace MariEngine.Tiles;

// TODO: Refactor this to use a generic base class for a component container
public class TileEntity(string name) : IAudioListener, IPriorityItem
{
    public string Name { get; set; } = name;

    public virtual int Priority { get; init; } = 0;
    
    private Coord position;

    public Coord Position
    {
        get => position;
        set
        {
            var oldPosition = position;
            position = value;
            
            foreach (var component in components)
                component.OnPositionUpdate();
            
            PositionUpdated?.Invoke(this, oldPosition, position);
            
            if (TriggerStepEvents)
                Tilemap.StepOn(this, position);
        }
    }
    
    public Vector2 SmoothedPosition { get; private set; }
    public float PositionSmoothing { get; set; }
    
    public bool TriggerStepEvents { get; set; }

    public Tilemap Tilemap { get; private set; }
    
    private readonly SortedSet<TileEntityComponent> components = new(new PriorityComparer<TileEntityComponent>());

    public delegate void PositionUpdatedHandler(TileEntity sender, Coord oldPos, Coord newPos);
    public event PositionUpdatedHandler PositionUpdated;

    public T AddComponent<T>() where T : TileEntityComponent
    {
        AssertComponent<T>();
        
        var component = Activator.CreateInstance<T>();
        AttachComponent(component);
        return component;
    }

    public T GetComponent<T>() where T : TileEntityComponent
    {
        var component = components.FirstOrDefault(c => c.GetType().IsAssignableTo(typeof(T)));
        return (T)component;
    }

    public void RemoveComponent<T>() where T : TileEntityComponent
    {
        var component = GetComponent<T>();
        if (component is null) return;

        components.Remove(component);
    }

    public void RemoveComponent<T>(T component) where T : TileEntityComponent
    {
        components.Remove(component);
    }
 
    public void AttachComponent<T>(T component) where T : TileEntityComponent
    {
        AssertComponent<T>();

        components.Add(component);
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
        if (components.Any(component => component.GetType() == type))
            throw new Exception($"Tile entity {Name} already has a component of type {type}");
    }

    private void AssertExclusivity<T>() where T : TileEntityComponent
    {
        var exclusiveComponent = components.FirstOrDefault(c => c.GetType().IsAssignableTo(typeof(T)));
        if (typeof(T).IsDefined(typeof(ExclusiveAttribute)) && exclusiveComponent is not null)
            throw new Exception(
                $"Trying to add tile entity component of type {typeof(T).Name} to entity {Name}, but this component is exclusive with {exclusiveComponent.GetType().Name}");
    }
    
    public bool HasComponent<T>() where T : Component => components.Any(c => c.GetType().IsAssignableTo(typeof(T)));

    protected internal void InitializeComponents()
    {
        foreach (var component in components)
            component.Initialize();
    }
    
    
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
        foreach (var component in components)
            component.Update(gameTime);

        if (PositionSmoothing == 0)
            SmoothedPosition = (Vector2)Position;
        else
            SmoothedPosition = Vector2.Lerp(SmoothedPosition, (Vector2)Position,
            PositionSmoothing * (float)gameTime.ElapsedGameTime.TotalSeconds);
    }

    public void Move(int dx = 0, int dy = 0)
    {
        var newPos = Position + new Coord(dx, dy);
        newPos = new Coord(MathHelper.Clamp(newPos.X, 0, Tilemap.Width - 1),
            MathHelper.Clamp(newPos.Y, 0, Tilemap.Height - 1));
        Position = newPos;
    }

    public void Move(Coord delta) => Move(delta.X, delta.Y);

    public void Destroy()
    {
        OnDestroy();
        Tilemap.RemoveTileEntity(this);
    }

    internal void DestroyWithoutRemove()
    {
        OnDestroy();
    }

    protected virtual void OnDestroy()
    {
        foreach (var component in components)
            component.DestroyWithoutRemove();
        components.Clear();
    }

    public Vector2 GetPosition() => Tilemap.CoordToWorldPoint(Position);
}