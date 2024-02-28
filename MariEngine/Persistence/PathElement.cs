namespace MariEngine.Persistence;

public class PathElement(string name, PathElement parent)
{
    public string Name { get; protected set; } = name;
    public PathElement Parent { get; protected set; } = parent;

    public override string ToString()
    {
        if (Parent is null)
            return Name;
        return $"{Parent}/{Name}";
    }
}