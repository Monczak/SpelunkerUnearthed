using System;
using System.Collections.Generic;

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

    protected internal Dictionary<Direction, IComponentSelectable> NavigationOverrides { get; }
}

public interface IComponentSelectable<out T> : IComponentSelectable where T : ComponentNode
{
    public event Action<T> Selected;
    public event Action<T> Deselected;
}

public static class ComponentSelectableExtensions
{
    public static T WithNavigationOverride<T>(this T component, Direction direction, IComponentSelectable<T> target) where T : ComponentNode
    {
        ((IComponentSelectable<T>)component).NavigationOverrides[direction] = target;
        return component;
    }
}