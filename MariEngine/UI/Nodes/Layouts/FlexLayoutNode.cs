using Microsoft.Xna.Framework;

namespace MariEngine.UI.Nodes.Layouts;

public class FlexLayoutNode : LayoutNode
{
    public FlexDirection FlexDirection { get; set; } = FlexDirection.Row;
    public float FlexGap { get; set; } = 1;
}