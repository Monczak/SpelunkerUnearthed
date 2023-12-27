using MariEngine.Components;
using MariEngine.UI.Nodes;
using MariEngine.UI.Nodes.Layouts;

namespace MariEngine.UI;

public class Canvas : Component
{
    public LayoutNode Root { get; } = new FlexLayoutNode();
}