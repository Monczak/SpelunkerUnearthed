﻿using System.Linq;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpelunkerUnearthed.Engine;
using SpelunkerUnearthed.Engine.Components;
using SpelunkerUnearthed.Engine.Logging;
using SpelunkerUnearthed.Engine.Rendering;
using SpelunkerUnearthed.Engine.Services;
using SpelunkerUnearthed.Engine.Tiles;
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

        scene = new TestScene(Window, GraphicsDevice);
        
        base.Initialize();
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