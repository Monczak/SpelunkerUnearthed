using System;
using MariEngine.Tiles;

namespace MariEngine.UI.Nodes.Components;

public abstract class ComponentNode : CanvasNode
{
    public override CanvasNode AddChild(CanvasNode node)
    {
        throw new ArgumentException("Component nodes cannot have children.");
    }
    
    public override void Accept(ICanvasRendererVisitor rendererVisitor, TileBufferFragment buffer)
    {
        rendererVisitor.Visit(this, buffer);
    }
}