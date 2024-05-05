using System;
using System.Diagnostics;
using System.Linq;
using FMOD.Studio;
using FmodForFoxes;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MariEngine;
using MariEngine.Audio;
using MariEngine.Components;
using MariEngine.Debugging;
using MariEngine.Events;
using MariEngine.Input;
using MariEngine.Logging;
using MariEngine.Persistence;
using MariEngine.Rendering;
using MariEngine.Services;
using MariEngine.Tiles;
using SpelunkerUnearthed.Scripts.Managers;
using SpelunkerUnearthed.Scripts.MapGeneration.Biomes;
using SpelunkerUnearthed.Scripts.MapGeneration.Features;
using SpelunkerUnearthed.Scripts.Scenes;
using SpelunkerUnearthed.Scripts.TileEntities;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using ContentPaths = SpelunkerUnearthed.Scripts.ContentPaths;
using Game = MariEngine.Game;

namespace SpelunkerUnearthed;

public class SpelunkerUnearthedGame : Game
{
    private DebugScreen debugScreen;
    private DebugScreenLine<TimeSpan> updateTimeDebugLine;
    private DebugScreenLine<TimeSpan> potentialUpdateTimeDebugLine;
    private DebugScreenLine<TimeSpan> drawTimeDebugLine;
    private DebugScreenLine<TimeSpan> potentialDrawTimeDebugLine;
    private DebugScreenLine<int> activeAudioEventsDebugLine;
    
    private Stopwatch updateTimeStopwatch;
    private Stopwatch drawTimeStopwatch;
    
    private Scene scene;

    public SpelunkerUnearthedGame() : base(new DesktopNativeFmodLibrary())
    {
        IsMouseVisible = true;

        Window.AllowUserResizing = true;
    }

    protected override void Initialize()
    {
        InitializeAudio(ContentPaths.Audio);
        
        FontSystemDefaults.FontResolutionFactor = 4.0f;
        FontSystemDefaults.KernelWidth = 4;
        FontSystemDefaults.KernelHeight = 4;

        updateTimeStopwatch = new Stopwatch();
        drawTimeStopwatch = new Stopwatch();
        
        ServiceRegistry.RegisterService(new FontProvider());
        ServiceRegistry.RegisterService(new RandomProvider());
        ServiceRegistry.RegisterService(new MaterialLoader());
        ServiceRegistry.RegisterService(new TileLoader());
        ServiceRegistry.RegisterService(new SpriteLoader());
        ServiceRegistry.RegisterService(new InputManager());
        ServiceRegistry.RegisterService(new AudioManager());
        ServiceRegistry.RegisterService(new BiomeLoader());
        ServiceRegistry.RegisterService(new FeatureLoader());
        ServiceRegistry.RegisterService(new EventManager());
        ServiceRegistry.RegisterService(new SaveLoadSystem("Saves"));
        
        ServiceRegistry.RegisterService(new TexturePool(Graphics.GraphicsDevice));
        
        // TODO: Load this from a config file
        ServiceRegistry.Get<FontProvider>().AddFont("Tiles", "Hack-Regular");
        ServiceRegistry.Get<FontProvider>().AddFont("Tiles", "Monospace");
        ServiceRegistry.Get<FontProvider>().AddFont("Monospace", "Hack-Regular");
        ServiceRegistry.Get<FontProvider>().AddFont("Monospace", "Monospace");
        ServiceRegistry.Get<FontProvider>().AddFont("UiFont", "whitrabt");
        
        debugScreen = new DebugScreen();
        ServiceRegistry.RegisterService(debugScreen);
        
        InitializeDebugScreen();

        InitializeInputs();

        ServiceRegistry.Get<AudioManager>().LoadBank(this, "Master");
        ServiceRegistry.Get<AudioManager>().LoadBank(this, "Master.strings");

        Window.ClientSizeChanged += (_, _) => ServiceRegistry.Get<EventManager>().Notify("ClientSizeChanged");
        
        // TODO: DEBUG - remove this
        using (var context = ServiceRegistry.Get<SaveLoadSystem>().LoadSaveFile("TestSave"))
        {
            var testSaveable = new TestSaveable { Number = 5, Text = "This is some random text" };
            context.Save(testSaveable, "TestData/TestSaveable");
        }
        
        using (var context = ServiceRegistry.Get<SaveLoadSystem>().LoadSaveFile("TestSave"))
        {
            var testSaveable = context.Load<TestSaveable>("TestData/TestSaveable");
            Logger.LogDebug($"{testSaveable.Number} - {testSaveable.Text}");
        }
        
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

        activeAudioEventsDebugLine = new DebugScreenLine<int>(count => $"Active audio events: {count}");

        debugScreen.AddLine(drawTimeDebugLine);
        debugScreen.AddLine(potentialDrawTimeDebugLine);
        debugScreen.AddLine(updateTimeDebugLine);
        debugScreen.AddLine(potentialUpdateTimeDebugLine);
        debugScreen.AddLine(activeAudioEventsDebugLine);
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
        
        inputManager.RegisterEvent(new InputEvent("UI_Up", Keys.Up));
        inputManager.RegisterEvent(new InputEvent("UI_Down", Keys.Down));
        inputManager.RegisterEvent(new InputEvent("UI_Left", Keys.Left));
        inputManager.RegisterEvent(new InputEvent("UI_Right", Keys.Right));
        inputManager.RegisterEvent(new InputEvent("UI_Select", Keys.Z));
        
        inputManager.RegisterEvent(new InputEvent("Mine", Keys.Z));
        inputManager.RegisterEvent(new InputEvent("Use", Keys.X));

        inputManager.OnPressed("ToggleDebugScreen", () => debugScreen.Enabled ^= true);
    }

    protected override void LoadContent()
    {
        SpriteBatch = new SpriteBatch(GraphicsDevice);
        
        ServiceRegistry.RegisterService(new TileAtlas(GraphicsDevice, SpriteBatch, 16));

        ServiceRegistry.Get<MaterialLoader>().LoadContent();
        
        ServiceRegistry.Get<TileLoader>().LoadContent();
        ServiceRegistry.Get<SpriteLoader>().LoadContent();
        
        ServiceRegistry.Get<TileAtlas>().SetTiles(ServiceRegistry.Get<TileLoader>().Content);
        ServiceRegistry.Get<TileAtlas>().CreateAtlas();
        
        ServiceRegistry.Get<BiomeLoader>().LoadContent();
        ServiceRegistry.Get<FeatureLoader>().LoadContent();
        
        scene = new TestScene(Window, Graphics);
        scene.Load();
        
        base.LoadContent();
    }
    
    protected override void UnloadContent()
    {
        ServiceRegistry.Get<AudioManager>().UnloadAllBanks(this);
        UnloadAudio();
        base.UnloadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        updateTimeStopwatch.Reset();
        updateTimeStopwatch.Start();
        
        ServiceRegistry.UpdateServices(gameTime);
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
        
        scene.Render(SpriteBatch);
        
        if (debugScreen.Enabled)
            debugScreen.Render(SpriteBatch);
        
        drawTimeStopwatch.Stop();
        potentialDrawTimeDebugLine.SetParams(drawTimeStopwatch.Elapsed);
        drawTimeDebugLine.SetParams(gameTime.ElapsedGameTime);
        
        activeAudioEventsDebugLine.SetParams(ServiceRegistry.Get<AudioManager>().ActiveEventCount);
        
        base.Draw(gameTime);
    }
}