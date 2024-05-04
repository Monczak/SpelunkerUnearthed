using System;
using MariEngine.Logging;
using MariEngine.Sprites;
using MariEngine.Tiles;

namespace MariEngine.UI.Nodes.Components;

public class ButtonComponent(Sprite background, string label = "") : ComponentNode, IComponentSelectable, IUiCommandReceiver
{
    public int TextPadding { get; init; } = 2;

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
                Logger.LogDebug($"Released {Label}");
                break;
            case InputKeyUiCommand inputKeyUiCommand:
                Logger.LogDebug($"Key {inputKeyUiCommand.Key}, pressed: {inputKeyUiCommand.IsPressed}");
                break;
        }
    }
}