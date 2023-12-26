namespace MariEngine.UI.Nodes.Components;

public abstract class ComponentNode : CanvasNode
{
    protected abstract Coord Size { get; }

    public override int? PreferredWidth => Size.X;
    public override int? PreferredHeight => Size.Y;
}