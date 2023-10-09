using System.Collections.Generic;
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

    private Dictionary<Room, CameraBounds> cameraBoundsMap;
    private int cameraBoundsOversize = 5;
    
    public int BaseTilemapSize => 16;
    
    public bool IsGenerating { get; private set; }

    public WorldManager(CaveSystemManager caveSystemManager, Tilemap tilemap, PlayerController playerController)
    {
        this.caveSystemManager = caveSystemManager;
        this.tilemap = tilemap;
        this.playerController = playerController;
        
        tilemapRenderer = tilemap.GetComponent<TilemapRenderer>();
        mapGenerator = tilemap.GetComponent<MapGenerator>();

        cameraBoundsMap = new Dictionary<Room, CameraBounds>();
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

        CoordBounds bounds = caveSystemManager.CurrentLevel.BoundingBox;
        tilemap.Resize(bounds.Size * BaseTilemapSize);
        tilemap.GetComponent<Transform>().Position = bounds.ExactCenter * BaseTilemapSize;
        
        GenerateMap();

        SetupRoomCameraBounds();
        
        stopwatch.Stop();
        IsGenerating = false;

        Logger.Log($"Cave generation completed in {stopwatch.Elapsed.TotalSeconds:F3} seconds");
    }

    private void SetupRoomCameraBounds()
    {
        cameraBoundsMap.Clear();

        foreach (CaveSystemLevel level in caveSystemManager.CaveSystem.Levels)
        {
            foreach (Room room in level.Rooms)
            {
                CoordBounds bounds = GetRoomBounds(room);
                Bounds worldBounds = Bounds.MakeCorners(tilemapRenderer.CoordToWorldPoint(bounds.TopLeft) - Vector2.One * cameraBoundsOversize,
                    tilemapRenderer.CoordToWorldPoint(bounds.BottomRight) + Vector2.One * cameraBoundsOversize);
                cameraBoundsMap[room] = new CameraBounds(worldBounds);
            }
        }
    }

    private void GenerateMap()
    {
        tilemap.Fill(ServiceRegistry.Get<TileLoader>().GetTile("Nothing"));
        
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
                playerController.OwnerEntity.Position = RoomPosToTilemapPos(room, room.PointsOfInterest[PointOfInterestType.PlayerSpawnPoint][0].Position);
        }
    }

    public Coord RoomPosToTilemapPos(Room room, Coord pos)
    {
        Coord boundsTopLeft = caveSystemManager.CurrentLevel.BoundingBox.TopLeft;
        return pos - (boundsTopLeft - room.Position) * BaseTilemapSize;
    }

    public Coord TilemapPosToRoomPos(Room room, Coord tilemapPos)
    {
        return tilemapPos - RoomPosToTilemapPos(room, Coord.Zero);
    }

    private Coord TransformRoomPos(Coord pos)
    {
        Coord boundsTopLeft = caveSystemManager.CurrentLevel.BoundingBox.TopLeft;
        return (pos - boundsTopLeft) * BaseTilemapSize;
    }

    public Room GetRoom(Coord tilemapPos)
    {
        foreach (Room room in caveSystemManager.CurrentLevel.Rooms)
        {
            CoordBounds roomBounds = GetRoomBounds(room);
            if (roomBounds.PointInside(tilemapPos))
                return room;
        }

        return null;
    }

    public CoordBounds GetRoomBounds(Room room)
    {
        return CoordBounds.MakeCorners(RoomPosToTilemapPos(room, Coord.Zero),
            RoomPosToTilemapPos(room, room.Size * BaseTilemapSize - Coord.One));
    }
    
    public CameraBounds GetCameraBounds(Coord tilemapPos)
    {
        Room room = GetRoom(tilemapPos);
        if (room is null) return null;
        return cameraBoundsMap[room];
    }
}