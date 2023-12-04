using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using MariEngine;
using MariEngine.Components;
using MariEngine.Debugging;
using MariEngine.Light;
using MariEngine.Logging;
using MariEngine.Services;
using MariEngine.Tiles;
using MariEngine.Utils;
using Microsoft.Xna.Framework;
using SpelunkerUnearthed.Scripts.MapGeneration;
using SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;
using SpelunkerUnearthed.Scripts.MapGeneration.MapProcessors;
using SpelunkerUnearthed.Scripts.TileEntities;
using SpelunkerUnearthed.Scripts.Utils;

namespace SpelunkerUnearthed.Scripts.Managers;

public class WorldManager : Component
{
    private Tilemap tilemap;
    private TilemapRenderer tilemapRenderer;
    private RoomMapGenerator roomMapGenerator;

    private PlayerController playerController;

    private Gizmos gizmos;

    public CaveSystemManager CaveSystemManager { get; }

    private Dictionary<Room, CameraBounds> cameraBoundsMap;
    private int cameraBoundsOversize = 5;

    private SortedList<int, IMapProcessor> mapProcessors;
    
    public int BaseTilemapSize => 16;
    
    public bool IsGenerating { get; private set; }

    public WorldManager(CaveSystemManager caveSystemManager, Tilemap tilemap, PlayerController playerController, Gizmos gizmos)
    {
        CaveSystemManager = caveSystemManager;
        this.tilemap = tilemap;
        this.playerController = playerController;
        this.gizmos = gizmos;
        
        tilemapRenderer = tilemap.GetComponent<TilemapRenderer>();
        roomMapGenerator = tilemap.GetComponent<RoomMapGenerator>();

        cameraBoundsMap = new Dictionary<Room, CameraBounds>();
        mapProcessors = new SortedList<int, IMapProcessor>();
    }

    public void AddProcessor(IMapProcessor processor, int priority)
    {
        mapProcessors.Add(priority, processor);
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
        var stopwatch = Logger.StartStopwatch();
        Logger.Log($"Cave generation started", stopwatch);

        IsGenerating = true;
            
        CaveSystemManager.Generate();

        CaveSystemManager.SetCurrentLevel(0);

        CoordBounds bounds = CaveSystemManager.CurrentLevel.BoundingBox;
        tilemap.Resize(bounds.Size * BaseTilemapSize);
        tilemap.GetComponent<Transform>().Position = bounds.ExactCenter * BaseTilemapSize;
        
        Logger.Log($"Cave generation: generating rooms", stopwatch);
        GenerateRooms();
        SetupRoomCameraBounds();
        
        Logger.Log($"Cave generation: processing map", stopwatch);
        var rooms = CaveSystemManager.CurrentLevel.Rooms;
        foreach (var (_, processor) in mapProcessors)
        {
            processor.ProcessMap(tilemap, rooms);
        }
        
        Logger.Log($"Cave generation: baking light map", stopwatch);
        tilemap.GetComponent<LightMap>().ForceUpdate();
        Logger.Log($"Cave generation: light map baking completed", stopwatch);
        
        IsGenerating = false;

        Logger.Log($"Cave generation completed", stopwatch);
    }

    private void SetupRoomCameraBounds()
    {
        cameraBoundsMap.Clear();

        foreach (CaveSystemLevel level in CaveSystemManager.CaveSystem.Levels)
        {
            foreach (Room room in level.Rooms)
            {
                CoordBounds bounds = RoomMath.GetRoomBounds(CaveSystemManager.CurrentLevel, room, BaseTilemapSize);
                Bounds worldBounds = Bounds.MakeCorners(tilemap.CoordToWorldPoint(bounds.TopLeft) - Vector2.One * cameraBoundsOversize,
                    tilemap.CoordToWorldPoint(bounds.BottomRight) + Vector2.One * cameraBoundsOversize);
                cameraBoundsMap[room] = new CameraBounds(worldBounds);
            }
        }
    }

    private void GenerateRooms()
    {
        foreach (Coord coord in tilemap.Coords)
        {
            tilemap.Place(CaveSystemManager.CaveSystem.BiomeMap.GetWall(coord), coord, Tilemap.BaseLayer);
            tilemap.Place(CaveSystemManager.CaveSystem.BiomeMap.GetGround(coord), coord, Tilemap.GroundLayer);
        }
        
        foreach (Room room in CaveSystemManager.CurrentLevel.Rooms)
        {
            RoomMapGenerationParameters parameters = new RoomMapGenerationParameters
            {
                NothingTile = ServiceRegistry.Get<TileLoader>().Get("Nothing"),
                BorderSize = 1,
                BorderGradientSize = 2,
                BorderGradientFillAmount = 0.6f,
            };
            
            roomMapGenerator.GenerateRoomMap(room, parameters, RoomMath.TransformRoomPos(CaveSystemManager.CurrentLevel, room.Position, BaseTilemapSize), CaveSystemManager.CaveSystem.BiomeMap, BaseTilemapSize);

            if ((room.Flags & RoomFlags.Entrance) != 0)
                playerController.OwnerEntity.Position = RoomMath.RoomPosToTilemapPos(CaveSystemManager.CurrentLevel, room, room.PointsOfInterest[PointOfInterestType.PlayerSpawnPoint][0].Position, BaseTilemapSize);
        }
    }

    public Room GetRoom(Coord tilemapPos)
    {
        foreach (Room room in CaveSystemManager.CurrentLevel.Rooms)
        {
            CoordBounds roomBounds = RoomMath.GetRoomBounds(CaveSystemManager.CurrentLevel, room, BaseTilemapSize);
            if (roomBounds.PointInside(tilemapPos))
                return room;
        }

        return null;
    }
    
    public CameraBounds GetRoomCameraBounds(Coord tilemapPos)
    {
        Room room = GetRoom(tilemapPos);
        if (room is null) return null;
        return cameraBoundsMap[room];
    }
    
    
    public void DrawLevel(int level)
    {
        foreach (Room room in CaveSystemManager.CaveSystem.Levels[level].Rooms)
        {
            Coord topLeft = RoomMath.TransformRoomPos(CaveSystemManager.CurrentLevel, room.Bounds.TopLeft, BaseTilemapSize);
            Coord topRight = RoomMath.TransformRoomPos(CaveSystemManager.CurrentLevel, room.Bounds.TopRight + Coord.UnitX, BaseTilemapSize);
            Coord bottomLeft = RoomMath.TransformRoomPos(CaveSystemManager.CurrentLevel, room.Bounds.BottomLeft + Coord.UnitY, BaseTilemapSize);
            Coord bottomRight = RoomMath.TransformRoomPos(CaveSystemManager.CurrentLevel, room.Bounds.BottomRight + Coord.One, BaseTilemapSize);

            Vector2 topLeftV = tilemap.CoordToWorldPoint(topLeft) + Vector2.One * 0.1f;
            Vector2 topRightV = tilemap.CoordToWorldPoint(topRight) + (-Vector2.UnitX + Vector2.UnitY) * 0.1f;
            Vector2 bottomLeftV = tilemap.CoordToWorldPoint(bottomLeft) + (Vector2.UnitX + -Vector2.UnitY)* 0.1f;
            Vector2 bottomRightV = tilemap.CoordToWorldPoint(bottomRight) + -Vector2.One * 0.1f;
            
            gizmos.DrawLine(topLeftV, topRightV, Color.Blue, lifetime: 0);
            gizmos.DrawLine(topRightV, bottomRightV, Color.Blue, lifetime: 0);
            gizmos.DrawLine(bottomRightV, bottomLeftV, Color.Blue, lifetime: 0);
            gizmos.DrawLine(bottomLeftV, topLeftV, Color.Blue, lifetime: 0);
            
            // gizmos.DrawRectangle((Vector2)room.Bounds.TopLeft + Vector2.One * 0.05f, (Vector2)room.Bounds.Size - Vector2.One * 0.1f,
            //     new Color(0, MathUtils.InverseLerp(20, 0, room.Distance), MathUtils.InverseLerp(20, 0, room.Distance), 0.1f), 0);
            foreach (SubRoomConnection connection in room.Connections)
            {
                Coord from = RoomMath.TransformRoomPos(CaveSystemManager.CurrentLevel, connection.From.Position, BaseTilemapSize) + Coord.One * BaseTilemapSize / 2;
                Coord to = RoomMath.TransformRoomPos(CaveSystemManager.CurrentLevel, connection.To.Position, BaseTilemapSize) + Coord.One * BaseTilemapSize / 2;
                Vector2 fromPos = tilemap.CoordToWorldPoint(from);
                Vector2 toPos = tilemap.CoordToWorldPoint(to);
                gizmos.DrawLine(fromPos + Vector2.One * 0.5f, toPos + Vector2.One * 0.5f, Color.Red, lifetime: 0);
            }
        }
    }
}