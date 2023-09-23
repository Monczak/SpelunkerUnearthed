﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MariEngine;
using MariEngine.Collision;
using MariEngine.Components;
using MariEngine.Light;
using MariEngine.Logging;
using MariEngine.Rendering;
using MariEngine.Services;
using MariEngine.Tiles;
using SpelunkerUnearthed.Scripts.Components;
using SpelunkerUnearthed.Scripts.MapGeneration;
using SpelunkerUnearthed.Scripts.TileEntities;

namespace SpelunkerUnearthed.Scripts.Scenes;

public class TestScene : Scene
{
    private Tilemap tilemap;
    private TilemapRenderer tilemapRenderer;
    private PlayerController playerController;
    
    public TestScene(GameWindow window, GraphicsDeviceManager graphics) : base(window, graphics)
    {
    }

    public override void Load()
    {
        LoadEntities();
        GenerateMap();
    }

    private void LoadEntities()
    {
        Entity cameraControllerEntity = new("Camera Controller");
        CameraController cameraController = new(Camera) { Smoothing = 10.0f };
        cameraControllerEntity.AttachComponent(cameraController);
        AddEntity(cameraControllerEntity);
        
        Entity tilemapEntity = new("Tilemap");
        tilemap = new Tilemap(64, 64);
        
        tilemapEntity.AttachComponent(new Transform());
        tilemapEntity.AttachComponent(tilemap);

        LightMap lightMap = new()
        {
            AmbientLight = new Color(20, 15, 17),
            // AmbientLight = Color.Black,
        };
        tilemapEntity.AttachComponent(lightMap);

        tilemapRenderer = new TilemapRenderer(graphics.GraphicsDevice, Camera);
        tilemapEntity.AttachComponent(tilemapRenderer);
        
        lightMap.AttachTilemapRenderer(tilemapRenderer);
        
        tilemapEntity.AttachComponent(new TilemapCollider());
        
        tilemapEntity.AttachComponent(new MapGenerator());

        // TODO: Place player in an appropriate spot (randomly, ensuring there is no wall where the player is supposed to spawn)
        // or next to a ladder that was taken to get to this level

        TileEntity player = new TileEntity("Player")
        {
            Tile = ServiceRegistry.Get<TileLoader>().GetTile("Player"),
            Position = new Coord(32, 32)
        };
        tilemap.AddTileEntity(player);
        
        playerController = new PlayerController();
        player.AttachComponent(playerController);
        
        player.AttachComponent(new LightEmitter { LightSource = new PointLight(new Color(237, 222, 138), 30) });

        AddEntity(tilemapEntity);
        
        cameraController.TrackTileEntity(player);
    }

    private void GenerateMap()
    {
        var mapGenerator = tilemap.GetComponent<MapGenerator>();

        MapGenerationParameters parameters = new MapGenerationParameters
        {
            Seed = 0,
            NothingTile = ServiceRegistry.Get<TileLoader>().GetTile("Nothing"),
            WallTile = ServiceRegistry.Get<TileLoader>().GetTile("Stone"),
            RandomFillAmount = 0.45f,
            SmoothIterations = 3,
            BorderSize = 1,
            BorderGradientSize = 2,
            BorderGradientFillAmount = 0.6f,
        };
        mapGenerator.GenerateMap(parameters);
    }
}