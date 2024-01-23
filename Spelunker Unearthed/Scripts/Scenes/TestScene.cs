using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MariEngine;
using MariEngine.Audio;
using MariEngine.Collision;
using MariEngine.Components;
using MariEngine.Debugging;
using MariEngine.Events;
using MariEngine.Light;
using MariEngine.Services;
using MariEngine.Tiles;
using MariEngine.UI;
using MariEngine.UI.Nodes.Layouts;
using SpelunkerUnearthed.Scripts.Components;
using SpelunkerUnearthed.Scripts.Managers;
using SpelunkerUnearthed.Scripts.MapGeneration;
using SpelunkerUnearthed.Scripts.MapGeneration.Biomes;
using SpelunkerUnearthed.Scripts.MapGeneration.MapProcessors;
using SpelunkerUnearthed.Scripts.TileEntities;

namespace SpelunkerUnearthed.Scripts.Scenes;

public class TestScene(GameWindow window, GraphicsDeviceManager graphics) : Scene(window, graphics)
{
    private Tilemap tilemap;
    private TilemapRenderer tilemapRenderer;
    private LightMap lightMap;
    private PlayerController playerController;

    private CaveSystemManager caveSystemManager;
    private WorldManager worldManager;

    private CameraController cameraController;

    private AmbienceController ambienceController;

    private Gizmos gizmos;
    
    private DebugScreenLine<Biome> biomeDebugLine = new(biome => $"Biome: {biome?.Name ?? "none"}");

    private Canvas canvas;

    public override void Load()
    {
        ServiceRegistry.Get<AudioManager>().LoadBank(this, "Ambience");
        
        LoadEntities();
        
        cameraController.TrackTileEntity(playerController.OwnerEntity);
        cameraController.SetBounds(100, tilemap.GetComponent<CameraBounds>());
        
        ambienceController.Play();

        const int seed = 0;
        ServiceRegistry.Get<RandomProvider>().Request(Constants.BiomeGenRng).Seed(seed);
        ServiceRegistry.Get<RandomProvider>().Request(Constants.MapGenRng).Seed(seed);
        ServiceRegistry.Get<RandomProvider>().Request(Constants.CaveSystemGenRng).Seed(seed);

        worldManager.StartCaveSystemLevelGenerationTask().ContinueWith(_ =>
        {
            // TestBiomeGeneration();
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
        var topLeft = tilemap.CoordToWorldPoint(bounds.TopLeft);
        var bottomRight = tilemap.CoordToWorldPoint(bounds.BottomRight) + Vector2.One;
        gizmos.DrawTexture(topLeft, bottomRight - topLeft, Color.White, texture, 10000f);
    }

    public override void Update(GameTime gameTime)
    {
        tilemapRenderer.Enabled = !worldManager.IsGenerating;
        lightMap.Enabled = !worldManager.IsGenerating;
        
        base.Update(gameTime);

        if (!worldManager.IsGenerating)
        {
            worldManager.DrawLevel(0);
            
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
        debugEntity.AttachComponent(new GizmoRenderer(graphics.GraphicsDevice, Camera)
            { Layer = 100, Enabled = false });
        AddEntity(debugEntity);

        Entity managersEntity = new("Managers");
        caveSystemManager = new CaveSystemManager();
        managersEntity.AttachComponent(caveSystemManager);

        Entity controllersEntity = new("Controllers");
        ambienceController = new AmbienceController();
        controllersEntity.AttachComponent(ambienceController);
        AddEntity(controllersEntity);

        Entity cameraControllerEntity = new("Camera Controller");
        cameraController = new CameraController(Camera) { Smoothing = 10f };
        cameraControllerEntity.AttachComponent(cameraController);
        AddEntity(cameraControllerEntity);

        Entity tilemapEntity = new("Tilemap");
        tilemap = new Tilemap(64, 64);
        tilemap.AddLayer(Tilemap.GroundLayer);

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

        RoomMapGenerator roomMapGenerator = new();
        roomMapGenerator.AddProcessor(new PlayerSpawnPointProcessor()); // TODO: Load all processors using reflection
        tilemapEntity.AttachComponent(roomMapGenerator);
        tilemapEntity.AttachComponent(new TilemapCameraBounds());

        TileEntity player = new TileEntity("Player")
        {
            Tile = ServiceRegistry.Get<TileLoader>().Get("Player")
        };
        tilemap.AddTileEntity(player);

        playerController = new PlayerController();
        player.AttachComponent(playerController);

        player.AttachComponent(new LightEmitter { LightSource = new PointLight(new Color(237, 222, 138), 1f, 30) });

        AddEntity(tilemapEntity);

        worldManager = new WorldManager(caveSystemManager, tilemap, playerController, gizmos);
        worldManager.AddProcessor(new RoomConnectionProcessor(worldManager.BaseRoomSize, gizmos), 0);
        
        player.AttachComponent(new PlayerBiomeWatcher(worldManager, ambienceController));

        managersEntity.AttachComponent(worldManager);

        Entity uiEntity = new("UI");
        canvas = new Canvas();
        uiEntity.AttachComponent(canvas);
        uiEntity.AttachComponent(new CanvasRenderer(graphics.GraphicsDevice, Camera));
        AddEntity(uiEntity);
        
        canvas.Root.AddChild(new FlexLayoutNode { FlexDirection = FlexDirection.Column, PreferredWidth = 5 });
        canvas.Root.AddChild(new FlexLayoutNode { FlexDirection = FlexDirection.Column, PreferredWidth = 20, PreferredHeight = 20});
        canvas.Root.AddChild(new FlexLayoutNode { FlexDirection = FlexDirection.Column, PreferredWidth = 5 });

        foreach (var child in canvas.Root.Children)
        {
            child.AddChild(new FlexLayoutNode());
            child.AddChild(new FlexLayoutNode());
        }
        
        canvas.GetComponent<CanvasRenderer>().Redraw();
    }
}