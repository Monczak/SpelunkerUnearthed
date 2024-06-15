using MariEngine.Loading;
using Microsoft.Xna.Framework;

namespace MariEngine.Components;

public abstract class Component : IPriorityItem
{
    [Special] public bool Enabled { get; set; } = true;
    
    public Entity OwnerEntity { get; private set; }
    
    [Special] public virtual int Priority { get; init; } = 0;

    public void SetOwner(Entity ownerEntity)
    {
        OwnerEntity = ownerEntity;
        OnAttach();
    }

    public T AddComponent<T>() where T : Component
    {
        return OwnerEntity.AddComponent<T>();
    }

    public T GetComponent<T>() where T : Component
    {
        return OwnerEntity?.GetComponent<T>();
    }

    internal void DoUpdate(GameTime gameTime)
    {
        if (Enabled)
            Update(gameTime);
    }

    protected virtual void Update(GameTime gameTime)
    {
        
    }

    protected virtual void OnAttach()
    {
        
    }

    protected internal virtual void Initialize()
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

public abstract class Component<TData> : Component where TData : ComponentData
{
    public abstract void Build(TData data);
}