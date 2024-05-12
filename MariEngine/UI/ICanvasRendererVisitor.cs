using MariEngine.Tiles;
using MariEngine.UI.Nodes;
using MariEngine.UI.Nodes.Components;
using MariEngine.UI.Nodes.Layouts;

namespace MariEngine.UI;

public interface ICanvasRendererVisitor
{
    void Render(CanvasNode node, TileBufferFragment buffer);
    void Render(LayoutNode node, TileBufferFragment buffer);
    void Render(ComponentNode node, TileBufferFragment buffer);
    void Render(TextComponent node, TileBufferFragment buffer);
    void Render(ButtonComponent node, TileBufferFragment buffer);
    void Render(SliderComponent node, TileBufferFragment buffer);
    void Render(InputFieldComponent node, TileBufferFragment buffer);
}