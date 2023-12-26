using System.Collections.Generic;

namespace MariEngine.UI.Nodes;

public abstract class CanvasNode
{
    public CanvasNode Parent { get; internal set; }
    private readonly List<CanvasNode> children = [];
    public IEnumerable<CanvasNode> Children => children.AsReadOnly();

    public virtual int? PreferredWidth { get; } = null;
    public virtual int? PreferredHeight { get; } = null;

    public void AddChild(CanvasNode node)
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