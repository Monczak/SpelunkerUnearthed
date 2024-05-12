using MariEngine.Tiles;
using Microsoft.Xna.Framework.Input;

namespace MariEngine.UI.Nodes.Components;

public class InputFieldComponent(int lineSpacing = 0) : SelectableComponentNode<InputFieldComponent>, IUiCommandReceiver
{
    private string text = "";

    public int LineSpacing { get; set; } = lineSpacing;

    public int CaretPos { get; private set; } = 1;

    public string Text
    {
        get => text;
        set
        {
            text = value;
            ValueChanged?.Invoke(this, text);
        }
    }
    
    public delegate void ValueChangedEventHandler(InputFieldComponent sender, string newValue);

    public event ValueChangedEventHandler ValueChanged;

    private void Type(string s)
    {
        Text = Text.Insert(CaretPos - 1, s);
        CaretPos += s.Length;
    }

    private void Backspace()
    {
        if (Text.Length == 0)
            return;
        Text = Text.Remove(CaretPos - 2, 1);
        CaretPos -= 1;
    }
    
    // TODO: Handle other keys (digits, punctuation, up/down arrows - update caret position accordingly)
    public void HandleCommand(UiCommand command)
    {
        switch (command)
        {
            case InputKeyUiCommand { Key: >= Keys.A and <= Keys.Z, IsPressed: true } c:
                Type(c.Key.ToString());
                break;
            case InputKeyUiCommand { Key: Keys.Back, IsPressed: true }:
                Backspace();
                break;
            case InputKeyUiCommand { Key: Keys.Space, IsPressed: true }:
                Type(" ");
                break;
            
            case InputKeyUiCommand { Key: Keys.Left, IsPressed: true }:
                CaretPos -= 1;
                if (CaretPos < 1) CaretPos = 1;
                break;
            case InputKeyUiCommand { Key: Keys.Right, IsPressed: true }:
                CaretPos += 1;
                if (CaretPos > Text.Length + 1) CaretPos = Text.Length + 1;
                break;
        }
    }

    public override void AcceptRenderer(ICanvasRendererVisitor rendererVisitor, TileBufferFragment buffer)
    {
        rendererVisitor.Render(this, buffer);
    }
}