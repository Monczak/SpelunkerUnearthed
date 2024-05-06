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
    public SelectableComponentNode SelectedComponent { get; private set; }
    private int selectedComponentIndex = 0;

    private Canvas canvas;
    private CanvasLayoutManager layoutManager;

    private Dictionary<SelectableComponentNode, Dictionary<Direction, SelectableComponentNode>> navigationGraph = new();
    
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

    // TODO: Make this smarter
    private void BuildNavigationGraph(IReadOnlyDictionary<CanvasNode, CoordBounds> layout)
    {
        Direction[] directions = [Direction.Up, Direction.Down, Direction.Left, Direction.Right];
        
        navigationGraph = new Dictionary<SelectableComponentNode, Dictionary<Direction, SelectableComponentNode>>();
        var allSelectables = components
            .OfType<SelectableComponentNode>()
            .ToList();
        
        foreach (var selectable in allSelectables)
        {
            navigationGraph[selectable] = new Dictionary<Direction, SelectableComponentNode>();

            if (!selectable.Selectable) continue;
            
            foreach (var direction in directions)
            {
                if (selectable.NavigationOverrides.TryGetValue(direction, out var target))
                {
                    navigationGraph[selectable][direction] = target;
                    continue;
                }
                
                if ((selectable.InhibitedNavigationDirections & direction) != 0)
                    continue;
                
                var nearestComponentInDirection = allSelectables
                    .Where(c => direction switch
                    {
                        Direction.Up => layout[c].BottomRight.Y <
                                        layout[selectable].TopLeft.Y,
                        Direction.Down => layout[c].TopLeft.Y >
                                          layout[selectable].BottomRight.Y,
                        Direction.Left => layout[c].BottomRight.X <
                                          layout[selectable].TopLeft.X,
                        Direction.Right => layout[c].TopLeft.X >
                                           layout[selectable].BottomRight.X,
                        _ => throw new ArgumentOutOfRangeException()
                    })
                    .MinBy(c => Math.Abs((layout[c].Center - layout[selectable].Center).X) + Math.Abs((layout[c].Center - layout[selectable].Center).Y));

                if (nearestComponentInDirection is null) continue;
                
                if (nearestComponentInDirection.Parent != selectable.Parent && nearestComponentInDirection.SelectFirstChild)
                    nearestComponentInDirection = nearestComponentInDirection.Parent.Children.OfType<SelectableComponentNode>().First();
                    
                navigationGraph[selectable][direction] = nearestComponentInDirection;
            }
        }
    }

    private void SelectComponent(Direction direction)
    {
        if (components.Count == 0)
            return;
        
        if (SelectedComponent is not null && !navigationGraph[SelectedComponent].ContainsKey(direction))
            return;
        
        SelectedComponent?.Deselect();

        SelectedComponent = SelectedComponent is null
            ? components.OfType<SelectableComponentNode>().FirstOrDefault()
            : navigationGraph[SelectedComponent][direction];

        SelectedComponent?.Select();
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