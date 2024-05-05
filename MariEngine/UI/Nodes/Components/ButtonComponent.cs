using System;
using System.Collections.Generic;
using MariEngine.Logging;
using MariEngine.Sprites;
using MariEngine.Tiles;

namespace MariEngine.UI.Nodes.Components;

public class ButtonComponent(Sprite background, Sprite inactiveBackground, string label = "") : ComponentNode, IComponentSelectable<ButtonComponent>, IUiCommandReceiver
{
    public int TextPadding { get; init; } = 2;

    public Sprite Background { get; set; } = background;
    public Sprite InactiveBackground { get; set; } = inactiveBackground;
    public string Label { get; set; } = label;
    
    public bool Pressed { get; private set; }
    
    public override void Accept(ICanvasRendererVisitor rendererVisitor, TileBufferFragment buffer)
    {
        rendererVisitor.Visit(this, buffer);
    }

    public event Action<ButtonComponent> Selected;
    public event Action<ButtonComponent> Deselected;
    public Direction InhibitedNavigationDirections { get; set; }
    public bool SelectFirstChild { get; init; } = false;
    public Dictionary<Direction, IComponentSelectable> NavigationOverrides { get; } = new();

    public bool IsSelected { get; set; }

    public void OnSelected()
    {
        Selected?.Invoke(this);
    }

    public void OnDeselected()
    {
        Deselected?.Invoke(this);
    }

    public bool Selectable { get; set; } = true;

    public void HandleCommand(UiCommand command)
    {
        switch (command)
        {
            case StartInteractionUiCommand:
                Pressed = true;
                Logger.LogDebug($"Pressed {Label}");
                break;
            case StopInteractionUiCommand:
                Pressed = false;
                break;
        }
    }
}