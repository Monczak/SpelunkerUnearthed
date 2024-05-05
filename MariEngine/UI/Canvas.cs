using System;
using System.Collections.Generic;
using MariEngine.Components;
using MariEngine.UI.Nodes;
using MariEngine.UI.Nodes.Components;
using MariEngine.UI.Nodes.Layouts;

namespace MariEngine.UI;

public class Canvas : Component
{
    private Dictionary<CanvasNode, CoordBounds> layout;
    public IReadOnlyDictionary<CanvasNode, CoordBounds> Layout => layout.AsReadOnly();
    
    public FlexLayoutNode Root { get; } = new();

    public Canvas()
    {
        Root.ChildAdded += RootOnChildAdded;
        Root.ChildDetached += RootOnChildDetached;
    }

    public void RecomputeLayout(Coord bufferSize)
    {
        layout = LayoutEngine.CalculateLayout(Root, bufferSize);
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