using System.Collections.Generic;
using MariEngine.Components;
using MariEngine.Input;
using MariEngine.Logging;
using MariEngine.Services;
using MariEngine.UI.Nodes.Components;

namespace MariEngine.UI;

public class CanvasNavigator : Component
{
    private List<ComponentNode> components = [];
    public IComponentSelectable SelectedComponent { get; private set; }
    private int selectedComponentIndex = 0;

    private Canvas canvas;
    
    protected override void OnAttach()
    {
        base.OnAttach();

        canvas = GetComponent<Canvas>();
        canvas.ComponentAdded += CanvasOnComponentAdded;
        canvas.ComponentRemoved += CanvasOnComponentRemoved;
        
        ServiceRegistry.Get<InputManager>().OnPressed("Up", SelectNextComponent);
        ServiceRegistry.Get<InputManager>().OnPressed("Down", SelectPreviousComponent);
        ServiceRegistry.Get<InputManager>().OnPressed("Mine", InteractOnPressed);
        ServiceRegistry.Get<InputManager>().OnReleased("Mine", InteractOnReleased);
    }

    private void SelectNextComponent() => SelectComponent(true);
    private void SelectPreviousComponent() => SelectComponent(false);
    private void InteractOnPressed() => (SelectedComponent as IUiCommandReceiver)?.HandleCommand(new UiCommand(UiCommandType.StartInteracting));
    private void InteractOnReleased() => (SelectedComponent as IUiCommandReceiver)?.HandleCommand(new UiCommand(UiCommandType.StopInteracting));

    private void CanvasOnComponentAdded(object sender, ComponentNode e)
    {
        components.Add(e);
    }
    
    private void CanvasOnComponentRemoved(object sender, ComponentNode e)
    {
        components.Remove(e);
    }

    private void SelectComponent(bool next)
    {
        if (components.Count == 0)
            return;

        SelectedComponent?.OnDeselected();
        
        do
        {
            selectedComponentIndex += next ? 1 : -1;
            selectedComponentIndex = (selectedComponentIndex + components.Count) % components.Count;
            SelectedComponent = components[selectedComponentIndex] as IComponentSelectable;
        } while (SelectedComponent is null);

        SelectedComponent?.OnSelected();
    }

    protected override void OnDestroy()
    {
        ServiceRegistry.Get<InputManager>().UnbindOnPressed("Up", SelectNextComponent);
        ServiceRegistry.Get<InputManager>().UnbindOnPressed("Down", SelectPreviousComponent);
        ServiceRegistry.Get<InputManager>().UnbindOnPressed("Mine", InteractOnPressed);
        ServiceRegistry.Get<InputManager>().UnbindOnReleased("Mine", InteractOnReleased);
        
        canvas.ComponentAdded -= CanvasOnComponentAdded;
        canvas.ComponentRemoved -= CanvasOnComponentRemoved;
        base.OnDestroy();
    }
}