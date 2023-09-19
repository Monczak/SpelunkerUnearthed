using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpelunkerUnearthed.Engine;
using SpelunkerUnearthed.Engine.Collision;
using SpelunkerUnearthed.Engine.Components;
using SpelunkerUnearthed.Engine.Light;
using SpelunkerUnearthed.Engine.Rendering;
using SpelunkerUnearthed.Engine.Services;
using SpelunkerUnearthed.Engine.Tiles;
using SpelunkerUnearthed.Scripts.MapGeneration;
using SpelunkerUnearthed.Scripts.TileEntities;

namespace SpelunkerUnearthed.Scripts.Scenes;

public class TestScene : Scene
{
    private Tilemap tilemap;
    private PlayerController playerController;
    
    public TestScene(GameWindow window, GraphicsDevice graphicsDevice) : base(window, graphicsDevice)
    {
    }

    public override void Load()
    {
        MakeTilemap();
        GenerateMap();
    }

    private void MakeTilemap()
    {
        Entity tilemapEntity = new("Tilemap");
        tilemap = new Tilemap(64, 64);
        
        tilemapEntity.AttachComponent(new Transform());
        tilemapEntity.AttachComponent(tilemap);
        tilemapEntity.AttachComponent(new LightMap
        {
            AmbientLight = new Color(20, 15, 17),
            // AmbientLight = Color.Black,
        });
        tilemapEntity.AttachComponent(new TilemapRenderer(graphicsDevice));
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
        
        player.AttachComponent(new PointLight { Color = new Color(237, 222, 138), Radius = 10 });

        AddEntity(tilemapEntity);
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