using MariEngine.Components;
using MariEngine.UI.Nodes;

namespace MariEngine.UI;

public class Canvas : Component
{
    public CanvasNode Root { get; } = new RootNode();
}