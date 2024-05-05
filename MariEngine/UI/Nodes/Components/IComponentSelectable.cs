using System;

namespace MariEngine.UI.Nodes.Components;

public interface IComponentSelectable
{
    void OnSelected();
    void OnDeselected();

    bool Selectable { get; set; }

    Direction InhibitedNavigationDirections => Direction.None;
}

public interface IComponentSelectable<out T> : IComponentSelectable where T : ComponentNode
{
    public event Action<T> Selected;
    public event Action<T> Deselected;
}