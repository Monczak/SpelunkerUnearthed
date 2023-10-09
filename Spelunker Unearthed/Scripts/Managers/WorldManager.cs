using System.Diagnostics;
using System.Threading.Tasks;
using MariEngine;
using MariEngine.Components;
using MariEngine.Logging;
using MariEngine.Services;
using MariEngine.Tiles;
using Microsoft.Xna.Framework;
using SpelunkerUnearthed.Scripts.MapGeneration;
using SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;
using SpelunkerUnearthed.Scripts.TileEntities;

namespace SpelunkerUnearthed.Scripts.Managers;

public class WorldManager : Component
{
    private Tilemap tilemap;
    private TilemapRenderer tilemapRenderer;
    private MapGenerator mapGenerator;

    private PlayerController playerController;

    private CaveSystemManager caveSystemManager;

    public int BaseTilemapSize => 16;
    
    public bool IsGenerating { get; private set; }

    public WorldManager(CaveSystemManager caveSystemManager, Tilemap tilemap, PlayerController playerController)
    {
        this.caveSystemManager = caveSystemManager;
        this.tilemap = tilemap;
        this.playerController = playerController;
        
        tilemapRenderer = tilemap.GetComponent<TilemapRenderer>();
        mapGenerator = tilemap.GetComponent<MapGenerator>();
    }

    public Task StartCaveSystemLevelGenerationTask()
    {
        if (IsGenerating) return null;
        return Task.Run(GenerateCaveSystemLevel);
    }

    private void GenerateCaveSystemLevel()
    {
        Logger.Log($"Cave generation started");
        Stopwatch stopwatch = new();

        IsGenerating = true;
        stopwatch.Start();
            
        caveSystemManager.Generate();

        caveSystemManager.SetCurrentLevel(0);

        CoordBounds bounds = caveSystemManager.CurrentLevel.CalculateBoundingBox();
        tilemap.Resize(bounds.Size * BaseTilemapSize);
        Logger.Log($"Size: {bounds.Size} Center: {bounds.ExactCenter}");
        tilemap.GetComponent<Transform>().Position = bounds.ExactCenter * BaseTilemapSize;
        
        GenerateMap();
            
        stopwatch.Stop();
        IsGenerating = false;

        Logger.Log($"Cave generation completed in {stopwatch.Elapsed.TotalSeconds:F3} seconds");
    }
    
    private void GenerateMap()
    {
        tilemap.Fill(ServiceRegistry.Get<TileLoader>().GetTile("Stone"));
        
        foreach (Room room in caveSystemManager.CurrentLevel.Rooms)
        {
            // TODO: Redesign the parameter thing so that rooms can have varying params inside
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
            
            mapGenerator.GenerateMap(room, parameters, TransformRoomPos(room.Position), BaseTilemapSize);

            if ((room.Flags & RoomFlags.Entrance) != 0)
                playerController.OwnerEntity.Position = TransformWorldPos(room.PointsOfInterest[PointOfInterestType.PlayerSpawnPoint][0].Position);
        }
    }

    private Coord TransformWorldPos(Coord pos)
    {
        Coord boundsTopLeft = caveSystemManager.CurrentLevel.CalculateBoundingBox().TopLeft;
        return pos - boundsTopLeft * BaseTilemapSize;
    }

    private Coord TransformRoomPos(Coord pos)
    {
        Coord boundsTopLeft = caveSystemManager.CurrentLevel.CalculateBoundingBox().TopLeft;
        return (pos - boundsTopLeft) * BaseTilemapSize;
    }
}