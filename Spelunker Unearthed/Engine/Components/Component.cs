using Microsoft.Xna.Framework;

namespace SpelunkerUnearthed.Engine.Components;

public abstract class Component
{
    private Entity ownerEntity;

    public Component()
    {
        
    }

    public void SetOwner(Entity ownerEntity)
    {
        this.ownerEntity = ownerEntity;
    }

    public T AddComponent<T>() where T : Component
    {
        return ownerEntity.AddComponent<T>();
    }

    public T GetComponent<T>() where T : Component
    {
        return ownerEntity.GetComponent<T>();
    }

    public virtual void Update(GameTime gameTime)
    {
        
    }
}