using System;

namespace MariEngine.UI.Nodes.Components;

public abstract class ComponentNode : CanvasNode
{
    public override void AddChild(CanvasNode node)
    {
        throw new Exception("Component nodes cannot have children.");
    }
}