using MariEngine.Tiles;
using MariEngine.UI.Nodes;
using MariEngine.UI.Nodes.Components;
using MariEngine.UI.Nodes.Layouts;

namespace MariEngine.UI;

public interface ICanvasRendererVisitor
{
    void Visit(CanvasNode node, TileBufferFragment buffer);
    void Visit(LayoutNode node, TileBufferFragment buffer);
    void Visit(ComponentNode node, TileBufferFragment buffer);
    void Visit(TextComponent node, TileBufferFragment buffer);
}