using System.Diagnostics;
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
using SpelunkerUnearthed.Scripts.Components;
using SpelunkerUnearthed.Scripts.MapGeneration;
using SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;
using SpelunkerUnearthed.Scripts.MapGeneration.MapProcessors;
using SpelunkerUnearthed.Scripts.TileEntities;

namespace SpelunkerUnearthed.Scripts.Scenes;

public class TestScene : Scene
{
    private Tilemap tilemap;
    private TilemapRenderer tilemapRenderer;
    private PlayerController playerController;

    private CaveSystemManager caveSystemManager;

    private Gizmos gizmos;
    
    public TestScene(GameWindow window, GraphicsDeviceManager graphics) : base(window, graphics)
    {
    }

    public override void Load()
    {
        LoadEntities();
        
        // TODO: Refactor this out to a separate class
        Task.Run(() =>
        {
            Logger.Log($"Cave generation started");
            Stopwatch stopwatch = new();
            stopwatch.Start();
            
            tilemapRenderer.Enabled = false;
            GenerateCaveSystem();

            caveSystemManager.SetCurrentLevel(0);
            caveSystemManager.SetCurrentRoomToEntrance();
        
            GenerateMap(caveSystemManager.CurrentRoom);
            
            stopwatch.Stop();
            tilemapRenderer.Enabled = true;

            return stopwatch;
        }).ContinueWith(task =>
        {
            Logger.Log($"Cave generation completed in {task.Result.Elapsed.TotalSeconds:F3} seconds");
        });
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        // caveSystemManager.DrawLevel(0);
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
        CameraController cameraController = new(Camera) { Smoothing = 10.0f };
        cameraControllerEntity.AttachComponent(cameraController);
        AddEntity(cameraControllerEntity);
        
        Entity tilemapEntity = new("Tilemap");
        tilemap = new Tilemap(64, 64);
        
        tilemapEntity.AttachComponent(new Transform());
        tilemapEntity.AttachComponent(tilemap);

        LightMap lightMap = new()
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
            Tile = ServiceRegistry.Get<TileLoader>().GetTile("Player")
        };
        tilemap.AddTileEntity(player);
        
        playerController = new PlayerController();
        player.AttachComponent(playerController);
        
        player.AttachComponent(new LightEmitter { LightSource = new PointLight(new Color(237, 222, 138), 30) });

        AddEntity(tilemapEntity);
        
        cameraController.TrackTileEntity(player);
        cameraController.SetBounds(tilemapEntity.GetComponent<CameraBounds>());
    }

    private void GenerateCaveSystem()
    {
        caveSystemManager.Generate();
    }

    private void GenerateMap(Room room)
    {
        var mapGenerator = tilemap.GetComponent<MapGenerator>();

        MapGenerationParameters parameters = new MapGenerationParameters
        {
            Seed = 0,
            NothingTile = ServiceRegistry.Get<TileLoader>().GetTile("Nothing"),
            WallTile = ServiceRegistry.Get<TileLoader>().GetTile("Stone"),
            RandomFillAmount = 0.4f,
            SmoothIterations = 3,
            BorderSize = 1,
            BorderGradientSize = 2,
            BorderGradientFillAmount = 0.6f,
        };
        mapGenerator.GenerateMap(room, parameters);

        playerController.OwnerEntity.Position = room.PointsOfInterest[PointOfInterestType.PlayerSpawnPoint][0].Position;
    }
}