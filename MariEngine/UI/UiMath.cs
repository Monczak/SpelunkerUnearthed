namespace MariEngine.UI;

public static class UiMath
{
    public static CoordBounds ApplyPadding(CoordBounds bounds, Coord padding) =>
        CoordBounds.MakeCorners(bounds.TopLeft + padding, bounds.BottomRight - padding);
}