using System;
using System.Collections.Generic;

namespace MariEngine.UI.Nodes.Components;

public abstract class SelectableComponentNode : ComponentNode
{
    public bool IsSelected { get; private set; }
    
    public virtual void Select()
    {
        IsSelected = true;
        OnSelected();
    }

    public virtual void Deselect()
    {
        IsSelected = false;
        OnDeselected();
    }

    protected virtual void OnSelected()
    {
        
    }

    protected virtual void OnDeselected()
    {
        
    }

    public bool Selectable { get; set; } = true;

    public virtual Direction InhibitedNavigationDirections => Direction.None;
    public bool SelectFirstChild { get; init; } = false;

    protected internal Dictionary<Direction, SelectableComponentNode> NavigationOverrides { get; } = new();
}

public abstract class SelectableComponentNode<T> : SelectableComponentNode where T : SelectableComponentNode
{
    public event Action<T> Selected;
    public event Action<T> Deselected;

    public override void Select()
    {
        Selected?.Invoke(this as T);
        base.Select();
    }
    
    public override void Deselect()
    {
        Deselected?.Invoke(this as T);
        base.Deselect();
    }
    
    public SelectableComponentNode<T> WithNavigationOverride(Direction direction, SelectableComponentNode target)
    {
        NavigationOverrides[direction] = target;
        return this;
    }
}