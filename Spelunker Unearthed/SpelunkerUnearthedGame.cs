using System;
using System.Diagnostics;
using System.Linq;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MariEngine;
using MariEngine.Components;
using MariEngine.Debugging;
using MariEngine.Input;
using MariEngine.Logging;
using MariEngine.Rendering;
using MariEngine.Services;
using MariEngine.Tiles;
using SpelunkerUnearthed.Scripts.MapGeneration.Biomes;
using SpelunkerUnearthed.Scripts.MapGeneration.Features;
using SpelunkerUnearthed.Scripts.Scenes;
using SpelunkerUnearthed.Scripts.TileEntities;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SpelunkerUnearthed;

public class SpelunkerUnearthedGame : Game
{
    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;

    private DebugScreen debugScreen;
    private DebugScreenLine<TimeSpan> updateTimeDebugLine;
    private DebugScreenLine<TimeSpan> potentialUpdateTimeDebugLine;
    private DebugScreenLine<TimeSpan> drawTimeDebugLine;
    private DebugScreenLine<TimeSpan> potentialDrawTimeDebugLine;
    
    private Stopwatch updateTimeStopwatch;
    private Stopwatch drawTimeStopwatch;
    
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

        updateTimeStopwatch = new Stopwatch();
        drawTimeStopwatch = new Stopwatch();
        
        ServiceRegistry.RegisterService(new FontProvider());
        ServiceRegistry.RegisterService(new RandomProvider());
        ServiceRegistry.RegisterService(new TileLoader());
        ServiceRegistry.RegisterService(new InputManager());
        ServiceRegistry.RegisterService(new BiomeLoader());
        ServiceRegistry.RegisterService(new FeatureLoader());
        
        ServiceRegistry.RegisterService(new TexturePool(graphics.GraphicsDevice));
        
        // TODO: Load this from a config file
        ServiceRegistry.Get<FontProvider>().AddFont("Tiles", "Hack-Regular");
        ServiceRegistry.Get<FontProvider>().AddFont("Tiles", "Monospace");
        ServiceRegistry.Get<FontProvider>().AddFont("Monospace", "Hack-Regular");
        ServiceRegistry.Get<FontProvider>().AddFont("Monospace", "Monospace");
        
        debugScreen = new DebugScreen();
        ServiceRegistry.RegisterService(debugScreen);
        
        InitializeDebugScreen();

        InitializeInputs();
        
        base.Initialize();
    }

    private void InitializeDebugScreen()
    {
        updateTimeDebugLine = new DebugScreenLine<TimeSpan>(timeSpan =>
            $"{1.0f / timeSpan.TotalSeconds:F3} UPS ({timeSpan.TotalMilliseconds:F3} ms per update)");
        potentialUpdateTimeDebugLine = new DebugScreenLine<TimeSpan>(timeSpan =>
            $"Potential UPS: {1.0f / timeSpan.TotalSeconds:F3} UPS ({timeSpan.TotalMilliseconds:F3} ms per update)");

        drawTimeDebugLine = new DebugScreenLine<TimeSpan>(timeSpan =>
            $"{1.0f / timeSpan.TotalSeconds:F3} FPS ({timeSpan.TotalMilliseconds:F3} ms per frame)");
        potentialDrawTimeDebugLine = new DebugScreenLine<TimeSpan>(timeSpan =>
            $"Potential FPS: {1.0f / timeSpan.TotalSeconds:F3} FPS ({timeSpan.TotalMilliseconds:F3} ms per frame)");

        debugScreen.AddLine(drawTimeDebugLine);
        debugScreen.AddLine(potentialDrawTimeDebugLine);
        debugScreen.AddLine(updateTimeDebugLine);
        debugScreen.AddLine(potentialUpdateTimeDebugLine);
    }

    private void InitializeInputs()
    {
        // TODO: Load inputs from a config file
        InputManager inputManager = ServiceRegistry.Get<InputManager>();
        
        inputManager.RegisterEvent(new InputEvent("ToggleDebugScreen", Keys.F3));
        inputManager.RegisterEvent(new InputEvent("ToggleGizmos", Keys.F4));
        
        inputManager.RegisterEvent(new InputEvent("Up", Keys.Up));
        inputManager.RegisterEvent(new InputEvent("Down", Keys.Down));
        inputManager.RegisterEvent(new InputEvent("Left", Keys.Left));
        inputManager.RegisterEvent(new InputEvent("Right", Keys.Right));
        inputManager.RegisterEvent(new InputEvent("Mine", Keys.Z));
        inputManager.RegisterEvent(new InputEvent("Use", Keys.X));

        inputManager.OnPressed("ToggleDebugScreen", () => debugScreen.Enabled ^= true);
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        
        ServiceRegistry.RegisterService(new TileAtlas(GraphicsDevice, spriteBatch, 16));

        ServiceRegistry.Get<TileLoader>().LoadContent();
        
        ServiceRegistry.Get<TileAtlas>().SetTiles(ServiceRegistry.Get<TileLoader>().Content);
        ServiceRegistry.Get<TileAtlas>().CreateAtlas();
        
        ServiceRegistry.Get<BiomeLoader>().LoadContent();
        ServiceRegistry.Get<FeatureLoader>().LoadContent();
        
        scene = new TestScene(Window, graphics);
        scene.Load();
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        updateTimeStopwatch.Reset();
        updateTimeStopwatch.Start();
        
        ServiceRegistry.UpdateServices();
        scene.Update(gameTime);

        updateTimeStopwatch.Stop();
        potentialUpdateTimeDebugLine.SetParams(updateTimeStopwatch.Elapsed);
        updateTimeDebugLine.SetParams(gameTime.ElapsedGameTime);
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        drawTimeStopwatch.Reset();
        drawTimeStopwatch.Start();
        
        GraphicsDevice.Clear(Color.Black);
        
        scene.Render(spriteBatch);
        
        if (debugScreen.Enabled)
            debugScreen.Render(spriteBatch);
        
        drawTimeStopwatch.Stop();
        potentialDrawTimeDebugLine.SetParams(drawTimeStopwatch.Elapsed);
        drawTimeDebugLine.SetParams(gameTime.ElapsedGameTime);
        
        base.Draw(gameTime);
    }
}