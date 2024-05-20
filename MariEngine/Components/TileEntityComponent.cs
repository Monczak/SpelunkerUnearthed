using MariEngine.Tiles;
using Microsoft.Xna.Framework;

namespace MariEngine.Components;

// TODO: Refactor this to use a generic base class for all components
public class TileEntityComponent
{
    public TileEntity OwnerEntity { get; private set; }
    
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

    protected internal virtual void OnPositionUpdate()
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