using System;
using System.Collections.Generic;
using MariEngine.UI.Nodes;
using MariEngine.UI.Nodes.Components;
using MariEngine.UI.Nodes.Layouts;

namespace MariEngine.UI;

public static class LayoutEngine
{
    public static Dictionary<CanvasNode, CoordBounds> CalculateLayout(CanvasNode rootNode)
    {
        Dictionary<CanvasNode, CoordBounds> boundsMap = new();

        return boundsMap;
    }

    private static Coord DetermineSize(CanvasNode node)
    {
        switch (node)
        {
            case ComponentNode componentNode:
                if (componentNode.PreferredWidth is null || componentNode.PreferredHeight is null)
                    throw new Exception("Component node preferred size is null, somehow?");
                return new Coord(componentNode.PreferredWidth.Value, componentNode.PreferredHeight.Value);
            case LayoutNode layoutNode:
                throw new NotImplementedException();
        } 
    }
}