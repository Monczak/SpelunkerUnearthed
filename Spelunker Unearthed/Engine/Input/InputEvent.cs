using Microsoft.Xna.Framework.Input;

namespace SpelunkerUnearthed.Engine.Input;

public class InputEvent
{
    public string Name { get; init; }
    public Keys Key { get; set; }

    public InputEvent(string name, Keys key)
    {
        Name = name;
        Key = key;
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}