using System;
using MariEngine.Components;
using MariEngine.UI.Nodes;
using MariEngine.UI.Nodes.Components;
using MariEngine.UI.Nodes.Layouts;

namespace MariEngine.UI;

public class Canvas : Component
{
    public FlexLayoutNode Root { get; } = new();

    public Canvas()
    {
        Root.ChildAdded += RootOnChildAdded;
        Root.ChildDetached += RootOnChildDetached;
    }

    private void RootOnChildDetached(object sender, CanvasNode e)
    {
        if (e is ComponentNode componentNode) ComponentRemoved?.Invoke(sender, componentNode);
    }

    private void RootOnChildAdded(object sender, CanvasNode e)
    {
        if (e is ComponentNode componentNode) ComponentAdded?.Invoke(sender, componentNode);
    }

    public event EventHandler<ComponentNode> ComponentAdded;
    public event EventHandler<ComponentNode> ComponentRemoved;
}