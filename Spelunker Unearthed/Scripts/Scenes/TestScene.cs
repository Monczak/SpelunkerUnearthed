using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpelunkerUnearthed.Engine;
using SpelunkerUnearthed.Engine.Collision;
using SpelunkerUnearthed.Engine.Components;
using SpelunkerUnearthed.Engine.Rendering;
using SpelunkerUnearthed.Engine.Services;
using SpelunkerUnearthed.Engine.Tiles;
using SpelunkerUnearthed.Scripts.MapGeneration;
using SpelunkerUnearthed.Scripts.TileEntities;

namespace SpelunkerUnearthed.Scripts.Scenes;

public class TestScene : Scene
{
    private Tilemap tilemap;
    private Player player;
    
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
        tilemap = new Tilemap(30, 30);
        
        tilemapEntity.AttachComponent(new Transform());
        tilemapEntity.AttachComponent(tilemap);
        tilemapEntity.AttachComponent(new TilemapRenderer(graphicsDevice));
        tilemapEntity.AttachComponent(new TilemapCollider());
        
        tilemapEntity.AttachComponent(new MapGenerator());

        player = new Player { Tile = ServiceRegistry.Get<TileLoader>().GetTile("Player") };
        tilemap.AddTileEntity(player);

        AddEntity(tilemapEntity);
    }

    private void GenerateMap()
    {
        var mapGenerator = tilemap.GetComponent<MapGenerator>();

        MapGenerationParameters parameters = new MapGenerationParameters
        {
            NothingTile = ServiceRegistry.Get<TileLoader>().GetTile("Nothing"),
            WallTile = ServiceRegistry.Get<TileLoader>().GetTile("Stone"),
            RandomFillAmount = 0.5f,
            SmoothIterations = 3,
        };
        mapGenerator.GenerateMap(parameters);
    }
}