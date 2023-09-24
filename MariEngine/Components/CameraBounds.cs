namespace MariEngine.Components;

public class CameraBounds : Component
{
    private Bounds bounds;

    public virtual Bounds GetBounds() => bounds;
    public void SetBounds(Bounds bounds) => this.bounds = bounds;
}