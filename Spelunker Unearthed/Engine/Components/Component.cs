using Microsoft.Xna.Framework;

namespace SpelunkerUnearthed.Engine.Components;

public abstract class Component
{
    protected Entity OwnerEntity;

    public Component()
    {
        
    }

    public void SetOwner(Entity ownerEntity)
    {
        this.OwnerEntity = ownerEntity;
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

    public virtual void Update(GameTime gameTime)
    {
        
    }

    public virtual void OnAttach()
    {
        
    }

    public void Destroy()
    {
        OwnerEntity.RemoveComponent(this);
    }

    protected virtual void OnDestroy()
    {
        
    }
}