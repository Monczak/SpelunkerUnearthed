using System;
using System.Collections.Generic;
using MariEngine.Tiles;
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

    public event EventHandler<CanvasNode> ChildAdded;
    public event EventHandler<CanvasNode> ChildDetached; 

    public virtual CanvasNode AddChild(CanvasNode node)
    {
        node.Parent = this;
        children.Add(node);
        
        node.ChildAdded += NodeOnChildAdded;
        node.ChildDetached += NodeOnChildDetached;
        ChildAdded?.Invoke(this, node);
        return node;
    }

    private void NodeOnChildDetached(object sender, CanvasNode e)
    {
        ChildDetached?.Invoke(sender, e);
    }

    private void NodeOnChildAdded(object sender, CanvasNode e)
    {
        ChildAdded?.Invoke(sender, e);
    }

    public void RemoveChild(CanvasNode node)
    {
        node.Detach();
        ChildDetached?.Invoke(this, node);
        children.Remove(node);
    }

    private void Detach()
    {
        Parent.children.Remove(this);
        Parent = null;
    }

    // public void Reparent(CanvasNode newParent)
    // {
    //     Detach();
    //     newParent.AddChild(this);
    // }

    public virtual void Accept(ICanvasRendererVisitor rendererVisitor, TileBufferFragment buffer)
    {
        rendererVisitor.Visit(this, buffer);
    }
}