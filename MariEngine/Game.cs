using System;
using FmodForFoxes;
using MariEngine.Audio;
using MariEngine.Debugging;
using MariEngine.Events;
using MariEngine.Input;
using MariEngine.Services;
using MariEngine.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using INITFLAGS = FMOD.Studio.INITFLAGS;

namespace MariEngine;

public abstract class Game : Microsoft.Xna.Framework.Game
{
    private readonly INativeFmodLibrary nativeFmodLibrary;
    protected readonly GraphicsDeviceManager Graphics;
    protected SpriteBatch SpriteBatch;

    protected Scene CurrentScene;
    protected DebugScreen DebugScreen;

    protected Game(INativeFmodLibrary nativeFmodLibrary)
    {
        this.nativeFmodLibrary = nativeFmodLibrary;
        Graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = ContentPaths.Content;
    }

    protected override void Initialize()
    {
        InitializeGame();
        base.Initialize();
    }

    protected virtual void InitializeGame(string savePath = "Saves")
    {
        InitializeServices(savePath);
    }

    protected virtual void InitializeServices(string savePath)
    {
        ServiceRegistry.RegisterService(new FontProvider());
        ServiceRegistry.RegisterService(new RandomProvider());
        ServiceRegistry.RegisterService(new MaterialLoader());
        ServiceRegistry.RegisterService(new TileLoader());
        ServiceRegistry.RegisterService(new SpriteLoader());
        ServiceRegistry.RegisterService(new InputManager());
        ServiceRegistry.RegisterService(new AudioManager());
        ServiceRegistry.RegisterService(new EventManager());
        ServiceRegistry.RegisterService(new TweenManager());
        ServiceRegistry.RegisterService(new SaveLoadSystem(savePath));
        ServiceRegistry.RegisterService(new SceneManager());
        
        ServiceRegistry.RegisterService(new TexturePool(Graphics.GraphicsDevice));
        
        DebugScreen = new DebugScreen();
        ServiceRegistry.RegisterService(DebugScreen);
        
        Window.ClientSizeChanged += (_, _) => ServiceRegistry.Get<EventManager>().Notify("ClientSizeChanged");

        ServiceRegistry.Get<SceneManager>().SceneLoaded += (_, scene) => LoadScene(scene);
        ServiceRegistry.Get<SceneManager>().SceneTypeLoaded += (_, sceneType) => LoadScene(sceneType);
    }

    protected override void LoadContent()
    {
        base.LoadContent();
        
        SpriteBatch = new SpriteBatch(GraphicsDevice);
        ServiceRegistry.RegisterService(new TileAtlas(GraphicsDevice, SpriteBatch, 16));

        ServiceRegistry.Get<MaterialLoader>().LoadContent();
        
        ServiceRegistry.Get<TileLoader>().LoadContent();
        ServiceRegistry.Get<SpriteLoader>().LoadContent();
        
        ServiceRegistry.Get<TileAtlas>().SetTiles(ServiceRegistry.Get<TileLoader>().Content);
        ServiceRegistry.Get<TileAtlas>().CreateAtlas();
    }

    protected void InitializeAudio(string contentPath)
    {
        FmodManager.Init(nativeFmodLibrary, FmodInitMode.CoreAndStudio, contentPath, studioInitFlags: INITFLAGS.LIVEUPDATE);
    }

    protected void UnloadAudio()
    {
        FmodManager.Unload();
    }

    protected override void Update(GameTime gameTime)
    {
        ServiceRegistry.UpdateServices(gameTime);
        CurrentScene?.Update(gameTime);
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        CurrentScene.Render(SpriteBatch);
        
        if (DebugScreen.Enabled)
            DebugScreen.Render(SpriteBatch);
        
        base.Draw(gameTime);
    }

    protected void LoadScene(Scene scene)
    {
        CurrentScene?.Unload();
        
        CurrentScene = scene;
        CurrentScene.Load();
    }

    protected void LoadScene(Type type)
    {
        LoadScene(Activator.CreateInstance(type, Window, Graphics) as Scene);
    }
    
    protected void LoadScene<T>() where T : Scene
    {
        LoadScene((T)Activator.CreateInstance(typeof(T), Window, Graphics));
    }
}