using System;
using System.Collections.Generic;
using MariEngine.UI.Nodes.Components;

namespace MariEngine.UI.Nodes;

public abstract class CanvasNode
{
    public float FlexGrow { get; set; } = 1;
    
    public CanvasNode Parent { get; internal set; }
    private readonly List<CanvasNode> children = [];
    public IEnumerable<CanvasNode> Children => children.AsReadOnly();

    public virtual void AddChild(CanvasNode node)
    {
        node.Parent = this;
        children.Add(node);
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