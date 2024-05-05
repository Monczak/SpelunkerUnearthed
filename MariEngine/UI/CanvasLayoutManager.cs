using System;
using System.Collections.Generic;
using MariEngine.Components;
using MariEngine.UI.Nodes;

namespace MariEngine.UI;

public class CanvasLayoutManager : Component
{
    private Canvas canvas;
    
    private Dictionary<CanvasNode, CoordBounds> layout = new();
    public IReadOnlyDictionary<CanvasNode, CoordBounds> Layout => layout.AsReadOnly();
    public event Action<IReadOnlyDictionary<CanvasNode, CoordBounds>> LayoutRecomputed;
    
    public void RecomputeLayout(Coord bufferSize)
    {
        layout = LayoutEngine.CalculateLayout(canvas.Root, bufferSize);
        LayoutRecomputed?.Invoke(Layout);
    }

    protected override void OnAttach()
    {
        base.OnAttach();
        canvas = GetComponent<Canvas>();
    }
}