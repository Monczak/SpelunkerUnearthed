using System.Collections.Generic;
using MariEngine.Components;
using MariEngine.Input;
using MariEngine.Logging;
using MariEngine.Services;
using MariEngine.UI.Nodes.Components;
using Microsoft.Xna.Framework.Input;

namespace MariEngine.UI;

public class CanvasNavigator : Component
{
    private readonly List<ComponentNode> components = [];
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
        
        ServiceRegistry.Get<InputManager>().OnPressedPassThrough(PassPressedToSelectedComponent);
        ServiceRegistry.Get<InputManager>().OnReleasedPassThrough(PassReleasedToSelectedComponent);
    }
    
    private void PassPressedToSelectedComponent(Keys key) => (SelectedComponent as IUiCommandReceiver)?.HandleCommand(new InputKeyUiCommand(key, true));
    private void PassReleasedToSelectedComponent(Keys key) => (SelectedComponent as IUiCommandReceiver)?.HandleCommand(new InputKeyUiCommand(key, false));

    private void SelectNextComponent() => SelectComponent(true);
    private void SelectPreviousComponent() => SelectComponent(false);
    private void InteractOnPressed() => (SelectedComponent as IUiCommandReceiver)?.HandleCommand(new StartInteractionUiCommand());
    private void InteractOnReleased() => (SelectedComponent as IUiCommandReceiver)?.HandleCommand(new StopInteractionUiCommand());

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

        var oldIndex = selectedComponentIndex;
        do
        {
            selectedComponentIndex += next ? 1 : -1;
            selectedComponentIndex = (selectedComponentIndex + components.Count) % components.Count;
            SelectedComponent = components[selectedComponentIndex] as IComponentSelectable;
        } while ((SelectedComponent is null || !SelectedComponent.Selectable) && selectedComponentIndex != oldIndex);

        SelectedComponent?.OnSelected();
    }

    protected override void OnDestroy()
    {
        ServiceRegistry.Get<InputManager>().UnbindOnPressed("Up", SelectNextComponent);
        ServiceRegistry.Get<InputManager>().UnbindOnPressed("Down", SelectPreviousComponent);
        ServiceRegistry.Get<InputManager>().UnbindOnPressed("Mine", InteractOnPressed);
        ServiceRegistry.Get<InputManager>().UnbindOnReleased("Mine", InteractOnReleased);
        
        ServiceRegistry.Get<InputManager>().UnbindOnPressedPassThrough(PassPressedToSelectedComponent);
        ServiceRegistry.Get<InputManager>().UnbindOnReleasedPassThrough(PassReleasedToSelectedComponent);
        
        canvas.ComponentAdded -= CanvasOnComponentAdded;
        canvas.ComponentRemoved -= CanvasOnComponentRemoved;
        base.OnDestroy();
    }
}