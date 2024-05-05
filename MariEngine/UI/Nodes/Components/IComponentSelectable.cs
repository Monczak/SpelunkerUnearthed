using System;

namespace MariEngine.UI.Nodes.Components;

public interface IComponentSelectable
{
    bool IsSelected { get; set; }
    
    internal void Select()
    {
        IsSelected = true;
        OnSelected();
    }

    internal void Deselect()
    {
        IsSelected = false;
        OnDeselected();
    }
    
    void OnSelected();
    void OnDeselected();

    bool Selectable { get; set; }

    Direction InhibitedNavigationDirections => Direction.None;
    bool SelectFirstChild { get; init; }
}

public interface IComponentSelectable<out T> : IComponentSelectable where T : ComponentNode
{
    public event Action<T> Selected;
    public event Action<T> Deselected;
}