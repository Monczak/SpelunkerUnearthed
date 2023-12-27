using System;
using System.Collections.Generic;
using System.Linq;
using MariEngine.UI.Nodes;
using MariEngine.UI.Nodes.Components;
using MariEngine.UI.Nodes.Layouts;

namespace MariEngine.UI;

public static class LayoutEngine
{
    private static Dictionary<CanvasNode, CoordBounds> boundsMap;
    public static Dictionary<CanvasNode, int> DepthMap { get; private set; }

    public static Dictionary<CanvasNode, CoordBounds> CalculateLayout(LayoutNode rootNode, Coord screenSize)
    {
        boundsMap = new Dictionary<CanvasNode, CoordBounds>
        {
            [rootNode] = new(Coord.Zero, screenSize)
        };
        DepthMap = new Dictionary<CanvasNode, int>
        {
            [rootNode] = 0
        };

        CalculateLayoutForNode(rootNode);

        return boundsMap;
    }

    private static void CalculateLayoutForNode(CanvasNode node, int depth = 1)
    {
        DepthMap[node] = depth;
        
        CoordBounds usableBounds = node switch
        {
            LayoutNode layoutNode => UiMath.ApplyPadding(boundsMap[layoutNode], layoutNode.Padding),
            _ => boundsMap[node],
        };
        
        var totalFlexGrow = node.Children.Sum(child => child.FlexGrow);

        if (node is FlexLayoutNode flexLayoutNode)
        {
            var flexDirection = flexLayoutNode.FlexDirection;
            Coord childPos = usableBounds.TopLeft;
            foreach (var child in node.Children)
            {
                Coord childSize = flexDirection switch
                {
                    FlexDirection.Row => new Coord(usableBounds.Size.X / totalFlexGrow * child.FlexGrow - flexLayoutNode.FlexGap,
                        usableBounds.Size.Y),
                    FlexDirection.Column => new Coord(usableBounds.Size.X,
                        usableBounds.Size.Y / totalFlexGrow * child.FlexGrow - flexLayoutNode.FlexGap),
                    _ => throw new ArgumentOutOfRangeException()
                };
                boundsMap[child] = new CoordBounds(childPos, childSize);
                
                childPos += flexDirection switch
                {
                    FlexDirection.Row => Coord.UnitX * (childSize.X + flexLayoutNode.FlexGap),
                    FlexDirection.Column => Coord.UnitY * (childSize.Y + flexLayoutNode.FlexGap),
                    _ => throw new ArgumentOutOfRangeException()
                };
                
                CalculateLayoutForNode(child, depth + 1);
            }
        }
    }
}