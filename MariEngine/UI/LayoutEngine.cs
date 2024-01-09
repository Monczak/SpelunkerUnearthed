using System;
using System.Collections.Generic;
using System.Linq;
using MariEngine.Logging;
using MariEngine.UI.Nodes;
using MariEngine.UI.Nodes.Components;
using MariEngine.UI.Nodes.Layouts;
using Microsoft.Xna.Framework;

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
        Vector2 error = Vector2.Zero;

        if (node is FlexLayoutNode flexLayoutNode)
        {
            var flexDirection = flexLayoutNode.FlexDirection;
            Coord childPos = usableBounds.TopLeft;
            for (int i = 0; i < node.Children.Count; i++)
            {
                var child = node.Children[i];
                float flexGapBefore = MathF.Ceiling(i == 0 ? 0 : flexLayoutNode.FlexGap / 2.0f);
                float flexGapAfter = MathF.Ceiling(i == node.Children.Count - 1 ? 0 : flexLayoutNode.FlexGap / 2.0f);

                float flexGap = flexGapBefore + flexGapAfter;
                
                Vector2 childSize = flexDirection switch
                {
                    FlexDirection.Row => new Vector2(usableBounds.Size.X / totalFlexGrow * child.FlexGrow - flexGap,
                        usableBounds.Size.Y),
                    FlexDirection.Column => new Vector2(usableBounds.Size.X,
                        usableBounds.Size.Y / totalFlexGrow * child.FlexGrow - flexGap),
                    _ => throw new ArgumentOutOfRangeException()
                };
                error += childSize - Vector2.Floor(childSize);
                if (error.X >= 0.5 || error.Y >= 0.5)
                {
                    var roundedError = Vector2.Round(error);
                    childSize += roundedError;
                    error -= roundedError;
                }
                
                childPos += flexDirection switch
                {
                    FlexDirection.Row => Coord.UnitX * flexGapBefore,
                    FlexDirection.Column => Coord.UnitY * flexGapBefore,
                    _ => throw new ArgumentOutOfRangeException()
                };
                
                boundsMap[child] = new CoordBounds(childPos, (Coord)childSize);
                
                childPos += flexDirection switch
                {
                    FlexDirection.Row => Coord.UnitX * (childSize.X + flexGapAfter),
                    FlexDirection.Column => Coord.UnitY * (childSize.Y + flexGapAfter),
                    _ => throw new ArgumentOutOfRangeException()
                };
                
                CalculateLayoutForNode(child, depth + 1);
            }
        }
    }
}