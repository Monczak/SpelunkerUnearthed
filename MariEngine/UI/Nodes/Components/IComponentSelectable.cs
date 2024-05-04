namespace MariEngine.UI.Nodes.Components;

public interface IComponentSelectable
{
    void OnSelected();
    void OnDeselected();

    bool Selectable { get; set; }
}