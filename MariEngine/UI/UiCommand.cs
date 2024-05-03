using Microsoft.Xna.Framework.Input;

namespace MariEngine.UI;

public record UiCommand;

public record StartInteractionUiCommand : UiCommand;

public record StopInteractionUiCommand : UiCommand;

public record InputKeyUiCommand(Keys Key, bool IsPressed) : UiCommand;