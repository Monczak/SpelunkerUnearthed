using MariEngine.Sprites;
using MariEngine.Tiles;

namespace MariEngine.UI.Nodes.Layouts;

public abstract class LayoutNode : CanvasNode
{
    public Coord Padding { get; set; }
    public Sprite Background { get; set; }
    
    public override void Accept(ICanvasRendererVisitor rendererVisitor, TileBufferFragment buffer)
    {
        rendererVisitor.Visit(this, buffer);
    }
}