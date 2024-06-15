using System;
using MariEngine.Loading;
using MariEngine.Services;
using MariEngine.Tiles;
using Microsoft.Xna.Framework;

namespace MariEngine.Components;

// TODO: Refactor this to use a generic base class for all components
public abstract class TileEntityComponent : IPriorityItem
{
    public TileEntity OwnerEntity { get; private set; }

    [Special] public virtual int Priority { get; init; } = 0;

    public void SetOwner(TileEntity ownerEntity)
    {
        OwnerEntity = ownerEntity;
        OnAttach();
    }
    
    public T AddComponent<T>() where T : TileEntityComponent
    {
        return OwnerEntity.AddComponent<T>();
    }

    public T GetComponent<T>() where T : TileEntityComponent
    {
        return OwnerEntity.GetComponent<T>();
    }
    
    public virtual void Update(GameTime gameTime)
    {
        
    }

    protected virtual void OnAttach()
    {
        
    }
    
    protected internal virtual void Initialize()
    {
        
    }

    protected internal virtual void OnPositionUpdate()
    {
        
    }

    public void Destroy()
    {
        OnDestroy();
        OwnerEntity.RemoveComponent(this);
    }

    internal void DestroyWithoutRemove()
    {
        OnDestroy();
    }

    protected virtual void OnDestroy()
    {
        
    }
}

public abstract class TileEntityComponent<TData> : TileEntityComponent where TData : ComponentData
{
    public abstract void Build(TData data);
}