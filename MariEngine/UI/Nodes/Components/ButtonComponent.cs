using MariEngine.Logging;
using MariEngine.Sprites;
using MariEngine.Tiles;

namespace MariEngine.UI.Nodes.Components;

public class ButtonComponent(Sprite background, string label = "") : ComponentNode, IComponentSelectable, IComponentInteractable
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

    public void OnPressed()
    {
        Pressed = true;
        Logger.LogDebug($"Pressed {Label}");
    }

    public void OnReleased()
    {
        Pressed = false;
        Logger.LogDebug($"Released {Label}");
    }
}