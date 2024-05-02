namespace MariEngine.UI.Nodes.Components;

public interface IUiCommandReceiver
{
    void HandleCommand(UiCommand command);
}