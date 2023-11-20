﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MariEngine;
using MariEngine.Collision;
using MariEngine.Components;
using MariEngine.Debugging;
using MariEngine.Light;
using MariEngine.Logging;
using MariEngine.Rendering;
using MariEngine.Services;
using MariEngine.Tiles;
using MariEngine.Utils;
using SpelunkerUnearthed.Scripts.Components;
using SpelunkerUnearthed.Scripts.Managers;
using SpelunkerUnearthed.Scripts.MapGeneration;
using SpelunkerUnearthed.Scripts.MapGeneration.Biomes;
using SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;
using SpelunkerUnearthed.Scripts.MapGeneration.MapProcessors;
using SpelunkerUnearthed.Scripts.TileEntities;

namespace SpelunkerUnearthed.Scripts.Scenes;

public class TestScene : Scene
{
    private Tilemap tilemap;
    private TilemapRenderer tilemapRenderer;
    private LightMap lightMap;
    private PlayerController playerController;

    private CaveSystemManager caveSystemManager;
    private WorldManager worldManager;

    private CameraController cameraController;

    private Gizmos gizmos;
    
    private DebugScreenLine<Biome> biomeDebugLine = new(biome => $"Biome: {biome?.Name ?? "none"}");
    
    public TestScene(GameWindow window, GraphicsDeviceManager graphics) : base(window, graphics)
    {
    }

    public override void Load()
    {
        LoadEntities();

        const int seed = 0;
        ServiceRegistry.Get<RandomProvider>().Request(Constants.BiomeGenRng).Seed(seed);
        ServiceRegistry.Get<RandomProvider>().Request(Constants.MapGenRng).Seed(seed);
        ServiceRegistry.Get<RandomProvider>().Request(Constants.CaveSystemGenRng).Seed(seed);

        worldManager.StartCaveSystemLevelGenerationTask().ContinueWith(_ =>
        {
            TestBiomeGeneration();
        });
        
        ServiceRegistry.Get<DebugScreen>().AddLine(biomeDebugLine);
    }

    private void TestBiomeGeneration()
    {
        Texture2D texture = ServiceRegistry.Get<TexturePool>().RequestTexture(new Coord(tilemap.Width, tilemap.Height), out _);
        Color[] data = new Color[tilemap.Width * tilemap.Height];

        Parallel.ForEach(tilemap.Coords, coord =>
        {
            (int x, int y) = coord;
            Color color = worldManager.CaveSystemManager.CaveSystem.BiomeMap.GetBiome(coord).Color;
            data[x + y * tilemap.Width] = new Color(color.R, color.G, color.B, (byte)20);
        });
        texture.SetData(data);

        var bounds = tilemap.Bounds;
        var topLeft = tilemapRenderer.CoordToWorldPoint(bounds.TopLeft);
        var bottomRight = tilemapRenderer.CoordToWorldPoint(bounds.BottomRight) + Vector2.One;
        gizmos.DrawTexture(topLeft, bottomRight - topLeft, Color.White, texture, 10000f);
    }

    public override void Update(GameTime gameTime)
    {
        tilemapRenderer.Enabled = !worldManager.IsGenerating;
        lightMap.Enabled = !worldManager.IsGenerating;
        
        base.Update(gameTime);

        if (!worldManager.IsGenerating)
        {
            // caveSystemManager.DrawLevel(0, worldManager.BaseTilemapSize);
            
            // TODO: Maybe set this as a toggle?
            cameraController.SetBounds(0, worldManager.GetRoomCameraBounds(playerController.OwnerEntity.Position));
            
            biomeDebugLine.SetParams(caveSystemManager.CaveSystem.BiomeMap.GetBiome(playerController.OwnerEntity.Position));
        }
    }

    private void LoadEntities()
    {
        Entity debugEntity = new("Debug");
        gizmos = new Gizmos();
        debugEntity.AttachComponent(gizmos);
        debugEntity.AttachComponent(new GizmoRenderer(graphics.GraphicsDevice, Camera) { Layer = 100 });
        AddEntity(debugEntity);

        Entity managersEntity = new("Managers");
        caveSystemManager = new CaveSystemManager(gizmos);
        managersEntity.AttachComponent(caveSystemManager);
        
        Entity cameraControllerEntity = new("Camera Controller");
        cameraController = new(Camera) { Smoothing = 10.0f };
        cameraControllerEntity.AttachComponent(cameraController);
        AddEntity(cameraControllerEntity);
        
        Entity tilemapEntity = new("Tilemap");
        tilemap = new Tilemap(64, 64);
        tilemap.AddLayer(-1);
        
        tilemapEntity.AttachComponent(new Transform());
        tilemapEntity.AttachComponent(tilemap);

        lightMap = new LightMap
        {
            // AmbientLight = Color.White,
            AmbientLight = new Color(20, 15, 17),
            // AmbientLight = Color.Black,
        };
        tilemapEntity.AttachComponent(lightMap);

        tilemapRenderer = new TilemapRenderer(graphics.GraphicsDevice, Camera);
        tilemapEntity.AttachComponent(tilemapRenderer);
        
        lightMap.AttachTilemapRenderer(tilemapRenderer);
        
        tilemapEntity.AttachComponent(new TilemapCollider());

        MapGenerator mapGenerator = new();
        mapGenerator.AddProcessor(new PlayerSpawnPointProcessor()); // TODO: Load all processors using reflection
        tilemapEntity.AttachComponent(mapGenerator);
        tilemapEntity.AttachComponent(new TilemapCameraBounds());

        TileEntity player = new TileEntity("Player")
        {
            Tile = ServiceRegistry.Get<TileLoader>().Get("Player")
        };
        tilemap.AddTileEntity(player);
        
        playerController = new PlayerController();
        player.AttachComponent(playerController);
        
        player.AttachComponent(new LightEmitter { LightSource = new PointLight(new Color(237, 222, 138), 30) });

        AddEntity(tilemapEntity);
        
        worldManager = new WorldManager(caveSystemManager, tilemap, playerController);
        managersEntity.AttachComponent(worldManager);
        
        cameraController.TrackTileEntity(player);
        cameraController.SetBounds(100, tilemapEntity.GetComponent<CameraBounds>());
    }
}