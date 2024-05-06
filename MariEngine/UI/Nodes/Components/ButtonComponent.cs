using System;
using System.Collections.Generic;
using MariEngine.Logging;
using MariEngine.Sprites;
using MariEngine.Tiles;

namespace MariEngine.UI.Nodes.Components;

public class ButtonComponent(Sprite background, Sprite inactiveBackground, string label = "") : SelectableComponentNode<ButtonComponent>, IUiCommandReceiver
{
    public int TextPadding { get; init; } = 2;

    public Sprite Background { get; set; } = background;
    public Sprite InactiveBackground { get; set; } = inactiveBackground;
    public string Label { get; set; } = label;
    
    public bool Pressed { get; private set; }
    
    public override void AcceptRenderer(ICanvasRendererVisitor rendererVisitor, TileBufferFragment buffer)
    {
        rendererVisitor.Render(this, buffer);
    }

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