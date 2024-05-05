using MariEngine.Tiles;

namespace MariEngine.UI.Nodes.Components;

public class TextComponent(string text = "") : ComponentNode
{
    public string Text
    {
        get => text;
        set
        {
            text = value; 
            OnTextChanged();
        }
    }

    public WordWrap WordWrap { get; set; }
    public int LineSpacing { get; set; }

    private void OnTextChanged()
    {
        
    }
    
    public override void Accept(ICanvasRendererVisitor rendererVisitor, TileBufferFragment buffer)
    {
        rendererVisitor.Visit(this, buffer);
    }
}

public enum WordWrap
{
    None,
    Wrap
}