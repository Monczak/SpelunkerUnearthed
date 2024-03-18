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
using MariEngine.Logging;
using MariEngine.Services;
using MariEngine.Sprites;
using MariEngine.Tiles;
using MariEngine.UI;
using MariEngine.UI.Nodes.Components;
using MariEngine.UI.Nodes.Layouts;
using SpelunkerUnearthed.Scripts.Components;
using SpelunkerUnearthed.Scripts.Managers;
using SpelunkerUnearthed.Scripts.MapGeneration;
using SpelunkerUnearthed.Scripts.MapGeneration.Biomes;
using SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;
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

        const int seed = 1;
        worldManager.StartGenerateWorldTask(seed).ContinueWith(_ =>
        {
            worldManager.LoadWorld();
            var firstLevel = worldManager.CaveSystemManager.CaveSystem.Levels[0];
            return worldManager.StartLoadLevelTask(firstLevel);
        })
        .ContinueWith(task =>
        {
            if (task.IsFaulted)
                Logger.LogError($"Level loading failed: {task.Exception}");
        });
        
        // worldManager.LoadWorld();
        // var firstLevel = worldManager.CaveSystemManager.CaveSystem.Levels[0];
        // worldManager.StartLoadLevelTask(firstLevel);
        
        ServiceRegistry.Get<DebugScreen>().AddLine(biomeDebugLine);
    }

    private void TestBiomeGeneration()
    {
        Texture2D texture = ServiceRegistry.Get<TexturePool>().RequestTexture(new Coord(tilemap.Width, tilemap.Height), out _);
        Color[] data = new Color[tilemap.Width * tilemap.Height];

        Parallel.ForEach(tilemap.Coords, coord =>
        {
            (int x, int y) = coord;
            Color color = worldManager.CaveSystemManager.GetBiome(coord).Color;
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
            worldManager.DrawLevel(caveSystemManager.CurrentLevel);
            
            // TODO: Maybe set this as a toggle?
            cameraController.SetBounds(0, worldManager.GetRoomCameraBounds(worldManager.CaveSystemManager.CurrentLevel, playerController.OwnerEntity.Position));
            
            biomeDebugLine.SetParams(caveSystemManager.GetBiome(playerController.OwnerEntity.Position));
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
        caveSystemManager = new CaveSystemManager(new SimpleBiomeProvider(), new TestDecisionEngine(), [
            new LadderRoomProcessor()
        ]);
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

        tilemapEntity.AttachComponent(new TilemapCollider());
        tilemapEntity.AttachComponent(new TilemapCameraBounds());

        TileEntity player = new TileEntity("Player");
        tilemap.AddTileEntity(player);

        playerController = new PlayerController();
        player.AttachComponent(playerController);

        player.AttachComponent(new LightEmitter { LightSource = new PointLight(new Color(237, 222, 138), 1f, 30) });
        player.AttachComponent(new SpriteRenderer(ServiceRegistry.Get<SpriteLoader>().Get("TestSprite")));

        AddEntity(tilemapEntity);

        var testTileEntity = new TileEntity("TestTileEntity");
        testTileEntity.AttachComponent(new BasicTileEntityRenderer(ServiceRegistry.Get<TileLoader>().Get("Player")));
        tilemap.AddTileEntity(testTileEntity);

        worldManager = new WorldManager(caveSystemManager, tilemap, playerController, gizmos);
        worldManager.AddProcessor(new RoomConnectionProcessor(worldManager.BaseRoomSize, gizmos), 0);
        worldManager.AddProcessor(new LadderFeaturePlacementProcessor(worldManager.BaseRoomSize), -10);
        worldManager.AddRoomMapProcessor(new PlayerSpawnPointProcessor(), 0); // TODO: Load all processors using reflection
        worldManager.AddRoomMapProcessor(new LadderPlacementProcessor(), -10); 
        
        player.AttachComponent(new PlayerBiomeWatcher(worldManager, ambienceController));

        managersEntity.AttachComponent(worldManager);

        Entity uiEntity = new("UI");
        canvas = new Canvas();
        uiEntity.AttachComponent(canvas);
        uiEntity.AttachComponent(new CanvasRenderer(graphics.GraphicsDevice, Camera));
        AddEntity(uiEntity);
        
        // var col1 = canvas.Root.AddChild(new FlexLayoutNode { FlexDirection = FlexDirection.Column, PreferredWidth = 5, ContentAlignment = FlexContentAlignment.Center });
        // col1.AddChild(new FlexLayoutNode { PreferredWidth = 5, PreferredHeight = 5 });
        // col1.AddChild(new FlexLayoutNode { PreferredWidth = 5, PreferredHeight = 5 });
        
        // var squareContainer = canvas.Root.AddChild(new FlexLayoutNode { FlexDirection = FlexDirection.Column, PreferredWidth = 20 });
        // squareContainer.AddChild(new FlexLayoutNode());
        // var square = squareContainer.AddChild(new FlexLayoutNode { FlexDirection = FlexDirection.Column, PreferredWidth = 20, PreferredHeight = 20 });
        // squareContainer.AddChild(new FlexLayoutNode());
        
        // square.AddChild(new FlexLayoutNode { PreferredWidth = 5, PreferredHeight = 5 });
        // square.AddChild(new FlexLayoutNode { PreferredWidth = 5, PreferredHeight = 5 });
        
        // var col2 = canvas.Root.AddChild(new FlexLayoutNode { FlexDirection = FlexDirection.Column, PreferredWidth = 5, ContentAlignment = FlexContentAlignment.Center });
        // col2.AddChild(new FlexLayoutNode { PreferredWidth = 5, PreferredHeight = 5 });
        // col2.AddChild(new FlexLayoutNode { PreferredWidth = 5, PreferredHeight = 5 });

        // var container1 = canvas.Root.AddChild(new FlexLayoutNode
        //     { Background = ServiceRegistry.Get<SpriteLoader>().Get("UIBackground"), Padding = Coord.One });
        // var container2 = canvas.Root.AddChild(new FlexLayoutNode());
        //
        // var text = container1.AddChild(new TextComponent("According to all known laws of aviation, there is no way a bee should be able to fly. Its wings are too small to get its fat little body off the ground. The bee, of course, flies anyway. Because bees don't care what humans think is impossible.") { WordWrap = WordWrap.Wrap, LineSpacing = 1 });
        // var text2 = container2.AddChild(new TextComponent("Test 123"));
        
        canvas.GetComponent<CanvasRenderer>().Redraw();
    }
}