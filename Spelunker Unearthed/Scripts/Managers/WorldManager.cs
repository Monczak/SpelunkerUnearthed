using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using MariEngine;
using MariEngine.Components;
using MariEngine.Light;
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

    public CaveSystemManager CaveSystemManager { get; }

    private Dictionary<Room, CameraBounds> cameraBoundsMap;
    private int cameraBoundsOversize = 5;
    
    public int BaseTilemapSize => 16;
    
    public bool IsGenerating { get; private set; }

    public WorldManager(CaveSystemManager caveSystemManager, Tilemap tilemap, PlayerController playerController)
    {
        CaveSystemManager = caveSystemManager;
        this.tilemap = tilemap;
        this.playerController = playerController;
        
        tilemapRenderer = tilemap.GetComponent<TilemapRenderer>();
        mapGenerator = tilemap.GetComponent<MapGenerator>();

        cameraBoundsMap = new Dictionary<Room, CameraBounds>();
    }

    public Task StartCaveSystemLevelGenerationTask()
    {
        if (IsGenerating) return null;
        return Task.Run(GenerateCaveSystemLevel).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Logger.LogError($"Cave generation failed: {task.Exception}");
            }
        });
    }

    private void GenerateCaveSystemLevel()
    {
        Logger.Log($"Cave generation started");
        Stopwatch stopwatch = new();

        IsGenerating = true;
        stopwatch.Start();
            
        CaveSystemManager.Generate();

        CaveSystemManager.SetCurrentLevel(0);

        CoordBounds bounds = CaveSystemManager.CurrentLevel.BoundingBox;
        tilemap.Resize(bounds.Size * BaseTilemapSize);
        tilemap.GetComponent<Transform>().Position = bounds.ExactCenter * BaseTilemapSize;
        
        Logger.Log($"Cave generation: generating map");
        GenerateMap();
        SetupRoomCameraBounds();

        var checkpoint = stopwatch.Elapsed.TotalSeconds;
        Logger.Log($"Cave generation: baking light map");
        tilemap.GetComponent<LightMap>().ForceUpdate();
        Logger.Log($"Cave generation: light map baking completed in {stopwatch.Elapsed.TotalSeconds - checkpoint:F3} seconds");
        
        stopwatch.Stop();
        IsGenerating = false;

        Logger.Log($"Cave generation completed in {stopwatch.Elapsed.TotalSeconds:F3} seconds");
    }

    private void SetupRoomCameraBounds()
    {
        cameraBoundsMap.Clear();

        foreach (CaveSystemLevel level in CaveSystemManager.CaveSystem.Levels)
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
        foreach (Coord coord in tilemap.Coords)
        {
            tilemap.Place(CaveSystemManager.CaveSystem.BiomeMap.GetWall(coord), coord, Tilemap.BaseLayer);
            tilemap.Place(CaveSystemManager.CaveSystem.BiomeMap.GetGround(coord), coord, Tilemap.GroundLayer);
        }
        
        foreach (Room room in CaveSystemManager.CurrentLevel.Rooms)
        {
            MapGenerationParameters parameters = new MapGenerationParameters
            {
                NothingTile = ServiceRegistry.Get<TileLoader>().Get("Nothing"),
                BorderSize = 1,
                BorderGradientSize = 2,
                BorderGradientFillAmount = 0.6f,
            };
            
            mapGenerator.GenerateRoomMap(room, parameters, TransformRoomPos(room.Position), CaveSystemManager.CaveSystem.BiomeMap, BaseTilemapSize);

            if ((room.Flags & RoomFlags.Entrance) != 0)
                playerController.OwnerEntity.Position = RoomPosToTilemapPos(room, room.PointsOfInterest[PointOfInterestType.PlayerSpawnPoint][0].Position);
        }
    }

    public Coord RoomPosToTilemapPos(Room room, Coord pos)
    {
        Coord boundsTopLeft = CaveSystemManager.CurrentLevel.BoundingBox.TopLeft;
        return pos - (boundsTopLeft - room.Position) * BaseTilemapSize;
    }

    public Coord TilemapPosToRoomPos(Room room, Coord tilemapPos)
    {
        return tilemapPos - RoomPosToTilemapPos(room, Coord.Zero);
    }

    private Coord TransformRoomPos(Coord pos)
    {
        Coord boundsTopLeft = CaveSystemManager.CurrentLevel.BoundingBox.TopLeft;
        return (pos - boundsTopLeft) * BaseTilemapSize;
    }

    public Room GetRoom(Coord tilemapPos)
    {
        foreach (Room room in CaveSystemManager.CurrentLevel.Rooms)
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
    
    public CameraBounds GetRoomCameraBounds(Coord tilemapPos)
    {
        Room room = GetRoom(tilemapPos);
        if (room is null) return null;
        return cameraBoundsMap[room];
    }
}