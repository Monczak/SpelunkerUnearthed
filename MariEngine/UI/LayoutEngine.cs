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

        if (node is FlexLayoutNode flexLayoutNode)
        {
            PerformFlexLayout(depth, flexLayoutNode, usableBounds);
        }
        else if (node is ComponentNode componentNode)
        {
            boundsMap[componentNode] = usableBounds;
        }
    }

    private static void PerformFlexLayout(int depth, FlexLayoutNode flexLayoutNode, CoordBounds usableBounds)
    {
        var usableSize = AdjustForPreferredSizes(flexLayoutNode, usableBounds);
        
        CalculateLayoutForChildren(depth, flexLayoutNode, usableBounds, usableSize);
    }

    private static void CalculateLayoutForChildren(int depth, FlexLayoutNode flexLayoutNode, CoordBounds usableBounds,
        Coord usableSize)
    {
        var flexDirection = flexLayoutNode.FlexDirection;
        var contentAlignment = flexLayoutNode.ContentAlignment;
        // var totalFlexGrow = flexLayoutNode.Children.Where(child => !child.HasPreferredSize).Sum(child => child.FlexGrow);
        //
        // if (totalFlexGrow == 0)
        //     totalFlexGrow = flexLayoutNode.Children.Count;
        
        var totalFlexGrow = flexLayoutNode.Children.Sum(child =>
            child.HasPreferredSize
                ? (child.PreferredWidth is not null && flexDirection == FlexDirection.Column) ||
                  (child.PreferredHeight is not null && flexDirection == FlexDirection.Row)
                    ? 1
                    : 0
                : child.FlexGrow);
        
        Vector2 error = Vector2.Zero;

        var childSizes = new Vector2[flexLayoutNode.Children.Count];
        var flexGaps = new (float before, float after)[flexLayoutNode.Children.Count];

        CalculateFlexGaps();
        CalculateChildSizes();
        CalculateChildPositions();
        
        foreach (var child in flexLayoutNode.Children)
            CalculateLayoutForNode(child, depth + 1);
        
        return;

        void CalculateFlexGaps()
        {
            for (int i = 0; i < flexLayoutNode.Children.Count; i++)
            { 
                flexGaps[i] = CalculateFlexGap(flexLayoutNode, i);
            }
        }

        void CalculateChildSizes()
        {
            for (int i = 0; i < flexLayoutNode.Children.Count; i++)
            {
                var child = flexLayoutNode.Children[i];
                var (flexGapBefore, flexGapAfter) = flexGaps[i];
                float flexGap = flexGapBefore + flexGapAfter;

                Vector2 childSize = flexDirection switch
                {
                    FlexDirection.Row => new Vector2(
                        child.PreferredWidth is not null
                            ? (float)child.PreferredWidth
                            : usableSize.X / totalFlexGrow * child.FlexGrow - flexGap,
                        child.PreferredHeight is not null ? (float)child.PreferredHeight : usableSize.Y
                    ),
                    FlexDirection.Column => new Vector2(
                        child.PreferredWidth is not null ? (float)child.PreferredWidth : usableSize.X,
                        child.PreferredHeight is not null
                            ? (float)child.PreferredHeight
                            : usableSize.Y / totalFlexGrow * child.FlexGrow - flexGap
                    ),
                    _ => throw new ArgumentOutOfRangeException()
                };

                error += childSize - Vector2.Floor(childSize);
                var roundedError = Vector2.Round(error);
                childSize += roundedError;
                error -= roundedError;

                childSizes[i] = childSize;
            }
        }

        void CalculateChildPositions()
        {
            Coord childPos = (flexDirection, contentAlignment) switch
            {
                (FlexDirection.Row or FlexDirection.Column, FlexContentAlignment.Start or FlexContentAlignment.SpaceBetween) => usableBounds.TopLeft,
                (FlexDirection.Row, FlexContentAlignment.End) => usableBounds.TopRight - Coord.UnitX * (childSizes.Zip(flexGaps).Sum(sizeGaps => sizeGaps.First.X + sizeGaps.Second.before + sizeGaps.Second.after) - 1),    // TODO: Are we sure about subtracting 1 here?
                (FlexDirection.Column, FlexContentAlignment.End) => usableBounds.BottomLeft - Coord.UnitY * (childSizes.Zip(flexGaps).Sum(sizeGaps => sizeGaps.First.Y + sizeGaps.Second.before + sizeGaps.Second.after) - 1),
                
                // TODO: Fix off by one errors here
                (FlexDirection.Row, FlexContentAlignment.Center) => (usableBounds.TopLeft + usableBounds.TopRight - Coord.UnitX * (childSizes.Zip(flexGaps).Sum(sizeGaps => sizeGaps.First.X + sizeGaps.Second.before + sizeGaps.Second.after) - 1)) / 2,
                (FlexDirection.Column, FlexContentAlignment.Center) => (usableBounds.TopLeft + usableBounds.BottomLeft - Coord.UnitY * (childSizes.Zip(flexGaps).Sum(sizeGaps => sizeGaps.First.Y + sizeGaps.Second.before + sizeGaps.Second.after) - 1)) / 2,
                
                _ => throw new NotImplementedException()
            };
            
            // TODO: Add support for SpaceBetween (later SpaceAround and SpaceEvenly)
            for (int i = 0; i < flexLayoutNode.Children.Count; i++)
            {
                var (flexGapBefore, flexGapAfter) = flexGaps[i];
                var child = flexLayoutNode.Children[i];
                childPos += flexDirection switch
                {
                    FlexDirection.Row => Coord.UnitX * flexGapBefore,
                    FlexDirection.Column => Coord.UnitY * flexGapBefore,
                    _ => throw new ArgumentOutOfRangeException()
                };
                
                boundsMap[child] = new CoordBounds(childPos, (Coord)childSizes[i]);
                
                childPos += flexDirection switch
                {
                    FlexDirection.Row => Coord.UnitX * (childSizes[i].X + flexGapAfter),
                    FlexDirection.Column => Coord.UnitY * (childSizes[i].Y + flexGapAfter),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }
    }

    private static Coord AdjustForPreferredSizes(FlexLayoutNode flexLayoutNode, CoordBounds usableBounds)
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
                    
                // var (flexGapBefore, flexGapAfter) = CalculateFlexGap(flexLayoutNode, i);
                // usableSize -= flexDirection switch
                // {
                //     FlexDirection.Row => Coord.UnitX * (flexGapBefore + flexGapAfter),
                //     FlexDirection.Column => Coord.UnitY * (flexGapBefore + flexGapAfter),
                //     _ => throw new ArgumentOutOfRangeException()
                // };
            }
        }

        return usableSize;
    }

    private static (float flexGapBefore, float flexGapAfter) CalculateFlexGap(FlexLayoutNode node, int indexInParent)
    {
        float flexGapBefore = MathF.Ceiling(indexInParent == 0 ? 0 : node.FlexGap / 2.0f);
        float flexGapAfter = MathF.Ceiling(indexInParent == node.Children.Count - 1 ? 0 : node.FlexGap / 2.0f);
        return (flexGapBefore, flexGapAfter);
    }
}