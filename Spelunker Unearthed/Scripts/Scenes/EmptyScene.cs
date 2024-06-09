using MariEngine;
using MariEngine.Input;
using MariEngine.Services;
using Microsoft.Xna.Framework;

namespace SpelunkerUnearthed.Scripts.Scenes;

public class EmptyScene(GameWindow window, GraphicsDeviceManager graphics) : Scene(window, graphics)
{
    private void LoadTestScene()
    {
        ServiceRegistry.Get<SceneManager>().LoadScene<TestScene>();
    }
    
    public override void Load()
    {
        ServiceRegistry.Get<InputManager>().OnPressed(this, "Use", LoadTestScene);
    }

    public override void Unload()
    {
        ServiceRegistry.Get<InputManager>().UnbindAll(this);
    }
}