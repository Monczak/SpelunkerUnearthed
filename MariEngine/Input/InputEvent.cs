using Microsoft.Xna.Framework.Input;

namespace MariEngine.Input;

public class InputEvent(string name, Keys key)
{
    public string Name { get; init; } = name;
    public Keys Key { get; set; } = key;

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}