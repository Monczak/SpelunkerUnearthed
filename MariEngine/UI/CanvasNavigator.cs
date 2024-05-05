using System;
using System.Collections.Generic;
using System.Linq;
using MariEngine.Components;
using MariEngine.Input;
using MariEngine.Logging;
using MariEngine.Services;
using MariEngine.UI.Nodes;
using MariEngine.UI.Nodes.Components;
using Microsoft.Xna.Framework.Input;

namespace MariEngine.UI;

public class CanvasNavigator : Component
{
    private readonly List<ComponentNode> components = [];
    public IComponentSelectable SelectedComponent { get; private set; }
    private int selectedComponentIndex = 0;

    private Canvas canvas;
    private CanvasLayoutManager layoutManager;

    private Dictionary<IComponentSelectable, Dictionary<Direction, IComponentSelectable>> navigationGraph = new();
    
    protected override void OnAttach()
    {
        base.OnAttach();

        canvas = GetComponent<Canvas>();
        canvas.ComponentAdded += CanvasOnComponentAdded;
        canvas.ComponentRemoved += CanvasOnComponentRemoved;

        layoutManager = canvas.GetComponent<CanvasLayoutManager>();
        layoutManager.LayoutRecomputed += BuildNavigationGraph;
        
        ServiceRegistry.Get<InputManager>().OnPressed("UI_Up", SelectComponentUp);
        ServiceRegistry.Get<InputManager>().OnPressed("UI_Down", SelectComponentDown);
        ServiceRegistry.Get<InputManager>().OnPressed("UI_Left", SelectComponentLeft);
        ServiceRegistry.Get<InputManager>().OnPressed("UI_Right", SelectComponentRight);
        
        ServiceRegistry.Get<InputManager>().OnPressed("UI_Select", InteractOnPressed);
        ServiceRegistry.Get<InputManager>().OnReleased("UI_Select", InteractOnReleased);
        
        ServiceRegistry.Get<InputManager>().OnPressedPassThrough(PassPressedToSelectedComponent);
        ServiceRegistry.Get<InputManager>().OnReleasedPassThrough(PassReleasedToSelectedComponent);
    }
    
    private void PassPressedToSelectedComponent(Keys key) => (SelectedComponent as IUiCommandReceiver)?.HandleCommand(new InputKeyUiCommand(key, true));
    private void PassReleasedToSelectedComponent(Keys key) => (SelectedComponent as IUiCommandReceiver)?.HandleCommand(new InputKeyUiCommand(key, false));

    private void SelectComponentUp() => SelectComponent(Direction.Up);
    private void SelectComponentDown() => SelectComponent(Direction.Down);
    private void SelectComponentLeft() => SelectComponent(Direction.Left);
    private void SelectComponentRight() => SelectComponent(Direction.Right);
    
    
    private void InteractOnPressed() => (SelectedComponent as IUiCommandReceiver)?.HandleCommand(new StartInteractionUiCommand());
    private void InteractOnReleased() => (SelectedComponent as IUiCommandReceiver)?.HandleCommand(new StopInteractionUiCommand());

    private void CanvasOnComponentAdded(object sender, ComponentNode e)
    {
        components.Add(e);
        // BuildNavigationGraph();
    }
    
    private void CanvasOnComponentRemoved(object sender, ComponentNode e)
    {
        components.Remove(e);
        // BuildNavigationGraph();
    }

    // TODO: Make this smarter - select first node in parent when parents are different, etc.
    public void BuildNavigationGraph(IReadOnlyDictionary<CanvasNode, CoordBounds> layout)
    {
        Direction[] directions = [Direction.Up, Direction.Down, Direction.Left, Direction.Right];
        
        navigationGraph = new Dictionary<IComponentSelectable, Dictionary<Direction, IComponentSelectable>>();
        var allSelectables = components
            .OfType<IComponentSelectable>()
            .Where(c => c.Selectable)
            .ToList();
        
        foreach (var selectable in allSelectables)
        {
            var selectableNode = selectable as CanvasNode;
            navigationGraph[selectable] = new Dictionary<Direction, IComponentSelectable>();

            foreach (var direction in directions)
            {
                var nearestComponentInDirection = allSelectables
                    .Select(c => c as CanvasNode)
                    .Where(c => direction switch
                    {
                        Direction.Up => layout[c].BottomRight.Y <
                                        layout[selectableNode].TopLeft.Y,
                        Direction.Down => layout[c].TopLeft.Y >
                                          layout[selectableNode].BottomRight.Y,
                        Direction.Left => layout[c].BottomRight.X <
                                          layout[selectableNode].TopLeft.X,
                        Direction.Right => layout[c].TopLeft.X >
                                           layout[selectableNode].BottomRight.X,
                        _ => throw new ArgumentOutOfRangeException()
                    })
                    .MinBy(c => (layout[c].Center - layout[selectableNode].Center).SqrMagnitude);
                if (nearestComponentInDirection is not null)
                    navigationGraph[selectable][direction] = nearestComponentInDirection as IComponentSelectable;
            }
        }
    }

    private void SelectComponent(Direction direction)
    {
        if (components.Count == 0)
            return;
        
        if (SelectedComponent is not null && !navigationGraph[SelectedComponent].ContainsKey(direction))
            return;
        
        SelectedComponent?.OnDeselected();

        SelectedComponent = SelectedComponent is null
            ? components.OfType<IComponentSelectable>().FirstOrDefault()
            : navigationGraph[SelectedComponent][direction];

        SelectedComponent?.OnSelected();
    }

    protected override void OnDestroy()
    {
        ServiceRegistry.Get<InputManager>().UnbindOnPressed("UI_Up", SelectComponentUp);
        ServiceRegistry.Get<InputManager>().UnbindOnPressed("UI_Down", SelectComponentDown);
        ServiceRegistry.Get<InputManager>().UnbindOnPressed("UI_Left", SelectComponentLeft);
        ServiceRegistry.Get<InputManager>().UnbindOnPressed("UI_Right", SelectComponentRight);
        
        ServiceRegistry.Get<InputManager>().UnbindOnPressed("UI_Select", InteractOnPressed);
        ServiceRegistry.Get<InputManager>().UnbindOnReleased("UI_Select", InteractOnReleased);
        
        canvas.ComponentAdded -= CanvasOnComponentAdded;
        canvas.ComponentRemoved -= CanvasOnComponentRemoved;
        base.OnDestroy();
    }
}