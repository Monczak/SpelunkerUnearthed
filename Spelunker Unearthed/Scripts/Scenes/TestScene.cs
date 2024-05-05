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
using MariEngine.Rendering;
using MariEngine.Services;
using MariEngine.Sprites;
using MariEngine.Tiles;
using MariEngine.UI;
using MariEngine.UI.Nodes;
using MariEngine.UI.Nodes.Components;
using MariEngine.UI.Nodes.Layouts;
using SpelunkerUnearthed.Scripts.Audio;
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

    private TileEntity testTileEntity;
    
    public override void Load()
    {
        ServiceRegistry.Get<AudioManager>().LoadBank(this, "Ambience");
        ServiceRegistry.Get<AudioManager>().LoadBank(this, "SFX");
        
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
            
            var pos = new Coord(255, 337);
            if (tilemap.IsInBounds(pos))
                testTileEntity.Position = pos;
        }
    }

    private void LoadEntities()
    {
        Entity debugEntity = new("Debug");
        gizmos = new Gizmos();
        Gizmos.SetDefault(gizmos);
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

        tilemapEntity.AttachComponent(new TilemapCollider(spatialPartitionCellSize: Coord.One * 8));
        tilemapEntity.AttachComponent(new TilemapCameraBounds());
        
        tilemapEntity.AttachComponent(new TilemapAudio(new PositionalAudioSource()
            .WithEvent("Mine", ServiceRegistry.Get<AudioManager>().GetEvent("event:/Mining", oneShot: true))
            .WithTrait(new WorldReverbTrait(tilemap))
            .WithTrait(new WorldAttenuationTrait(tilemap))
        ));

        var player = new TileEntity("Player");
        ServiceRegistry.Get<AudioManager>().SetListener(player);
        
        player.AttachComponent(new TileEntitySpriteRenderer(ServiceRegistry.Get<SpriteLoader>().Get("Player")));
        player.AttachComponent(new TileEntitySpriteCollider());
        tilemap.AddTileEntity(player);

        playerController = new PlayerController();
        player.AttachComponent(playerController);
        player.AttachComponent(new LightEmitter { LightSource = new PointLight(new Color(237, 222, 138), 1f, 30) });

        AddEntity(tilemapEntity);

        testTileEntity = new TileEntity("TestTileEntity");
        testTileEntity.AttachComponent(new TileEntitySpriteRenderer(ServiceRegistry.Get<SpriteLoader>().Get("Player")));
        testTileEntity.AttachComponent(new TileEntitySpriteCollider());
        
        var testAudio = new TileEntityAudioSource(new PositionalAudioSource()
            .WithEvent("Test", ServiceRegistry.Get<AudioManager>().GetEvent("event:/Mining", oneShot: true))
            .WithTrait(new WorldReverbTrait(tilemap))
            .WithTrait(new WorldAttenuationTrait(tilemap))
        );
        testTileEntity.AttachComponent(testAudio);
        testTileEntity.AttachComponent(new AudioTester());
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
        uiEntity.AttachComponent(new CanvasLayoutManager());
        uiEntity.AttachComponent(new CanvasNavigator());
        uiEntity.AttachComponent(new CanvasRenderer(graphics.GraphicsDevice, Camera, redrawEveryFrame: true));
        AddEntity(uiEntity);

        var container = canvas.Root.AddChild(new FlexLayoutNode { FlexDirection = FlexDirection.Column });
        var spacer = container.AddChild(new FlexLayoutNode());
        var panel = container.AddChild(new FlexLayoutNode
            { Background = ServiceRegistry.Get<SpriteLoader>().Get("UIBackground"), Padding = Coord.One, PreferredHeight = 9, FlexGap = 3 });
        
        var button1 = panel.AddChild(new ButtonComponent(
            ServiceRegistry.Get<SpriteLoader>().Get("UIBackground"),
            ServiceRegistry.Get<SpriteLoader>().Get("DimUIBackground"),
            "Button 1") { PreferredHeight = 5, TextPadding = 1 });
        var button2 = panel.AddChild(new ButtonComponent(
            ServiceRegistry.Get<SpriteLoader>().Get("UIBackground"),
            ServiceRegistry.Get<SpriteLoader>().Get("DimUIBackground"),
            "Button 2") { PreferredHeight = 5, TextPadding = 1 });
        var button3 = panel.AddChild(new ButtonComponent(
            ServiceRegistry.Get<SpriteLoader>().Get("UIBackground"),
            ServiceRegistry.Get<SpriteLoader>().Get("DimUIBackground"),
            "Button 3") { PreferredHeight = 5, TextPadding = 1 });
        
        var sliderPanel = panel.AddChild(new FlexLayoutNode { FlexDirection = FlexDirection.Column });
        
        var slider1 = sliderPanel.AddChild(new SliderComponent(
            ServiceRegistry.Get<SpriteLoader>().Get("SliderBackground"),
            ServiceRegistry.Get<SpriteLoader>().Get("DimSliderBackground"),
            ServiceRegistry.Get<SpriteLoader>().Get("SliderBar"),
            0, 10) { PreferredHeight = 1, SelectFirstChild = true });
        var slider2 = sliderPanel.AddChild(new SliderComponent(
            ServiceRegistry.Get<SpriteLoader>().Get("SliderBackground"),
            ServiceRegistry.Get<SpriteLoader>().Get("DimSliderBackground"),
            ServiceRegistry.Get<SpriteLoader>().Get("SliderBar"),
            0, 10) { PreferredHeight = 1, SelectFirstChild = true });
        var slider3 = sliderPanel.AddChild(new SliderComponent(
            ServiceRegistry.Get<SpriteLoader>().Get("SliderBackground"),
            ServiceRegistry.Get<SpriteLoader>().Get("DimSliderBackground"),
            ServiceRegistry.Get<SpriteLoader>().Get("SliderBar"),
            0, 10) { PreferredHeight = 1, SelectFirstChild = true });
        
        canvas.GetComponent<CanvasRenderer>().Redraw(recomputeLayout: true);
    }
}