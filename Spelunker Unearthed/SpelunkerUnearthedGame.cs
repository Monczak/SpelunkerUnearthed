using System.Linq;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MariEngine;
using MariEngine.Components;
using MariEngine.Input;
using MariEngine.Logging;
using MariEngine.Rendering;
using MariEngine.Services;
using MariEngine.Tiles;
using SpelunkerUnearthed.Scripts.Scenes;
using SpelunkerUnearthed.Scripts.TileEntities;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SpelunkerUnearthed;

public class SpelunkerUnearthedGame : Game
{
    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;

    private Scene scene;

    public SpelunkerUnearthedGame()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        Window.AllowUserResizing = true;
    }

    protected override void Initialize()
    {
        FontSystemDefaults.FontResolutionFactor = 4.0f;
        FontSystemDefaults.KernelWidth = 4;
        FontSystemDefaults.KernelHeight = 4;
        
        ServiceRegistry.RegisterService(new TileLoader());
        ServiceRegistry.RegisterService(new InputManager());

        InitializeInputs();

        scene = new TestScene(Window, graphics);
        
        base.Initialize();
    }

    private void InitializeInputs()
    {
        // TODO: Load inputs from a config file
        InputManager inputManager = ServiceRegistry.Get<InputManager>();
        inputManager.RegisterEvent(new InputEvent("Up", Keys.Up));
        inputManager.RegisterEvent(new InputEvent("Down", Keys.Down));
        inputManager.RegisterEvent(new InputEvent("Left", Keys.Left));
        inputManager.RegisterEvent(new InputEvent("Right", Keys.Right));
        inputManager.RegisterEvent(new InputEvent("Mine", Keys.Z));
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        
        ServiceRegistry.RegisterService(new TileAtlas(GraphicsDevice, spriteBatch, 16));

        var tiles = ServiceRegistry.Get<TileLoader>().LoadTiles();
        ServiceRegistry.Get<TileAtlas>().CreateAtlas(tiles);
        
        scene.Load();
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        ServiceRegistry.UpdateServices();
        scene.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        
        scene.Render(spriteBatch);
        
        base.Draw(gameTime);
    }
}