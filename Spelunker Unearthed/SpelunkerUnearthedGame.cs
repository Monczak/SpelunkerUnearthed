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
    private DebugScreenLine<TimeSpan> updateTimeDebugLine;
    private DebugScreenLine<TimeSpan> potentialUpdateTimeDebugLine;
    private DebugScreenLine<TimeSpan> drawTimeDebugLine;
    private DebugScreenLine<TimeSpan> potentialDrawTimeDebugLine;
    private DebugScreenLine<int> activeAudioEventsDebugLine;
    
    private Stopwatch updateTimeStopwatch;
    private Stopwatch drawTimeStopwatch;

    public SpelunkerUnearthedGame() : base(new DesktopNativeFmodLibrary())
    {
        IsMouseVisible = true;

        Window.AllowUserResizing = true;
    }

    protected override void InitializeGame(string savePath = "Saves")
    {
        base.InitializeGame(savePath);
        
        InitializeAudio(ContentPaths.Audio);
        
        FontSystemDefaults.FontResolutionFactor = 4.0f;
        FontSystemDefaults.KernelWidth = 4;
        FontSystemDefaults.KernelHeight = 4;

        updateTimeStopwatch = new Stopwatch();
        drawTimeStopwatch = new Stopwatch();
        
        // TODO: Load this from a config file
        ServiceRegistry.Get<FontProvider>().AddFont("Tiles", "Hack-Regular");
        ServiceRegistry.Get<FontProvider>().AddFont("Tiles", "Monospace");
        ServiceRegistry.Get<FontProvider>().AddFont("Monospace", "Hack-Regular");
        ServiceRegistry.Get<FontProvider>().AddFont("Monospace", "Monospace");
        ServiceRegistry.Get<FontProvider>().AddFont("UiFont", "whitrabt");
        
        InitializeDebugScreen();
        InitializeInputs();

        ServiceRegistry.Get<AudioManager>().LoadBank(this, "Master");
        ServiceRegistry.Get<AudioManager>().LoadBank(this, "Master.strings");
    }

    protected override void InitializeServices(string savePath)
    {
        base.InitializeServices(savePath);
        
        ServiceRegistry.RegisterService(new BiomeLoader());
        ServiceRegistry.RegisterService(new FeatureLoader());
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

        DebugScreen.AddLine(drawTimeDebugLine);
        DebugScreen.AddLine(potentialDrawTimeDebugLine);
        DebugScreen.AddLine(updateTimeDebugLine);
        DebugScreen.AddLine(potentialUpdateTimeDebugLine);
        DebugScreen.AddLine(activeAudioEventsDebugLine);
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

        inputManager.OnPressed(this, "ToggleDebugScreen", () => DebugScreen.Enabled ^= true);
    }

    protected override void LoadContent()
    {
        base.LoadContent();
        
        ServiceRegistry.Get<BiomeLoader>().LoadContent();
        ServiceRegistry.Get<FeatureLoader>().LoadContent();
        
        LoadScene<TestScene>();
        // CurrentScene.Load();
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

        base.Update(gameTime);
        
        updateTimeStopwatch.Stop();
        potentialUpdateTimeDebugLine.SetParams(updateTimeStopwatch.Elapsed);
        updateTimeDebugLine.SetParams(gameTime.ElapsedGameTime);
                
        activeAudioEventsDebugLine.SetParams(ServiceRegistry.Get<AudioManager>().ActiveEventCount);
    }

    protected override void Draw(GameTime gameTime)
    {
        drawTimeStopwatch.Reset();
        drawTimeStopwatch.Start();
        
        base.Draw(gameTime);
        
        drawTimeStopwatch.Stop();
        potentialDrawTimeDebugLine.SetParams(drawTimeStopwatch.Elapsed);
        drawTimeDebugLine.SetParams(gameTime.ElapsedGameTime);
    }
}