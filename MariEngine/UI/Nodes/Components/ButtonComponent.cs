using System;
using MariEngine.Logging;
using MariEngine.Sprites;
using MariEngine.Tiles;

namespace MariEngine.UI.Nodes.Components;

public class ButtonComponent(Sprite background, string label = "") : ComponentNode, IComponentSelectable, IUiCommandReceiver
{
    public Sprite Background { get; set; } = background;
    public string Label { get; set; } = label;
    
    public bool Pressed { get; private set; }
    
    public override void Accept(ICanvasRendererVisitor rendererVisitor, TileBufferFragment buffer)
    {
        rendererVisitor.Visit(this, buffer);
    }

    public void OnSelected()
    {
        Logger.LogDebug($"Selected {Label}");
    }

    public void OnDeselected()
    {
        Logger.LogDebug($"Deselected {Label}");
    }

    public void HandleCommand(UiCommand command)
    {
        switch (command)
        {
            case { Type: UiCommandType.StartInteracting }:
                Pressed = true;
                Logger.LogDebug($"Pressed {Label}");
                break;
            case { Type: UiCommandType.StopInteracting }:
                Pressed = false;
                Logger.LogDebug($"Released {label}");
                break;
        }
    }
}