using Microsoft.Xna.Framework;
using SpelunkerUnearthed.Engine.Tiles;

namespace SpelunkerUnearthed.Engine.Components;

// TODO: Refactor this to use a generic base class for all components
public class TileEntityComponent
{
    protected TileEntity OwnerEntity;
    
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

    public void Destroy()
    {
        OwnerEntity.RemoveComponent(this);
    }

    protected virtual void OnDestroy()
    {
        
    }
}