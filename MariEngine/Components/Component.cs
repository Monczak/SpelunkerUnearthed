using Microsoft.Xna.Framework;

namespace MariEngine.Components;

public abstract class Component : IPriorityItem
{
    public bool Enabled { get; set; } = true;
    
    protected Entity OwnerEntity;
    
    public virtual int Priority { get; init; } = 0;

    public Component()
    {
        
    }

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
        return OwnerEntity.GetComponent<T>();
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

    public void Destroy()
    {
        OnDestroy();
        OwnerEntity.RemoveComponent(this);
    }

    protected virtual void OnDestroy()
    {
        
    }
}