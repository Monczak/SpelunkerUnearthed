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
        
        var totalFlexGrow = node.Children.Where(child => !child.HasPreferredSize).Sum(child => child.FlexGrow);
        Vector2 error = Vector2.Zero;

        if (node is FlexLayoutNode flexLayoutNode)
        {
            var flexDirection = flexLayoutNode.FlexDirection;
            
            Coord usableSize = usableBounds.Size;
            for (int i = 0; i < flexLayoutNode.Children.Count; i++)
            {
                var child = flexLayoutNode.Children[i];
                if (child.HasPreferredSize)
                {
                    usableSize -= flexDirection switch
                    {
                        FlexDirection.Row => Coord.UnitX * (child.PreferredWidth ?? 0),
                        FlexDirection.Column => Coord.UnitY * (child.PreferredHeight ?? 0),
                        _ => throw new ArgumentOutOfRangeException()
                    }; 
                    
                    var (flexGapBefore, flexGapAfter) = CalculateFlexGap(flexLayoutNode, i);
                    usableSize -= flexDirection switch
                    {
                        FlexDirection.Row => Coord.UnitX * (flexGapBefore + flexGapAfter),
                        FlexDirection.Column => Coord.UnitY * (flexGapBefore + flexGapAfter),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                }
            }
            
            Coord childPos = usableBounds.TopLeft;
            for (int i = 0; i < flexLayoutNode.Children.Count; i++)
            {
                var child = flexLayoutNode.Children[i];
                var (flexGapBefore, flexGapAfter) = CalculateFlexGap(flexLayoutNode, i);

                float flexGap = flexGapBefore + flexGapAfter;
                
                Vector2 childSize = flexDirection switch
                {
                    FlexDirection.Row => new Vector2(
                        child.PreferredWidth is not null ? (float)child.PreferredWidth : usableSize.X / totalFlexGrow * child.FlexGrow - flexGap,
                        child.PreferredHeight is not null ? (float)child.PreferredHeight : usableSize.Y
                    ),
                    FlexDirection.Column => new Vector2(
                        child.PreferredWidth is not null ? (float)child.PreferredWidth : usableSize.X,
                        child.PreferredHeight is not null ? (float)child.PreferredHeight : usableSize.Y / totalFlexGrow * child.FlexGrow - flexGap
                    ),
                    _ => throw new ArgumentOutOfRangeException()
                };
                
                error += childSize - Vector2.Floor(childSize);
                var roundedError = Vector2.Round(error);
                childSize += roundedError;
                error -= roundedError;
                
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

    private static (float flexGapBefore, float flexGapAfter) CalculateFlexGap(FlexLayoutNode node, int indexInParent)
    {
        float flexGapBefore = MathF.Ceiling(indexInParent == 0 ? 0 : node.FlexGap / 2.0f);
        float flexGapAfter = MathF.Ceiling(indexInParent == node.Children.Count - 1 ? 0 : node.FlexGap / 2.0f);
        return (flexGapBefore, flexGapAfter);
    }
}