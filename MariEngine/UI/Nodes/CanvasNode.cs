using System;
using System.Collections.Generic;
using MariEngine.UI.Nodes.Components;

namespace MariEngine.UI.Nodes;

public abstract class CanvasNode
{
    public float FlexGrow { get; set; } = 1;
        
    public int? PreferredWidth { get; set; }
    public int? PreferredHeight { get; set; }

    public bool HasPreferredSize => PreferredWidth is not null || PreferredHeight is not null;
    
    public CanvasNode Parent { get; internal set; }
    private readonly List<CanvasNode> children = [];
    public IReadOnlyList<CanvasNode> Children => children.AsReadOnly();

    public virtual CanvasNode AddChild(CanvasNode node)
    {
        node.Parent = this;
        children.Add(node);
        return node;
    }

    public void RemoveChild(CanvasNode node)
    {
        node.Detach();
        children.Remove(node);
    }

    public void Detach()
    {
        Parent.children.Remove(this);
        Parent = null;
    }

    public void Reparent(CanvasNode newParent)
    {
        Detach();
        newParent.AddChild(this);
    }
}