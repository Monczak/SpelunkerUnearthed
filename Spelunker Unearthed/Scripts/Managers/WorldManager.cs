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

public class WorldManager(CaveSystemManager caveSystemManager, Tilemap tilemap, PlayerController playerController,
        Gizmos gizmos)
    : Component
{
    private TilemapRenderer tilemapRenderer = tilemap.GetComponent<TilemapRenderer>();
    private RoomMapGenerator roomMapGenerator = tilemap.GetComponent<RoomMapGenerator>();

    public CaveSystemManager CaveSystemManager { get; } = caveSystemManager;

    private Dictionary<Room, CameraBounds> cameraBoundsMap = new();
    private int cameraBoundsOversize = 5;

    private SortedList<int, MapProcessor> mapProcessors = new();
    
    public int BaseRoomSize => 16;
    
    public bool IsGenerating { get; private set; }

    public void AddProcessor(MapProcessor processor, int priority)
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
        tilemap.Resize(bounds.Size * BaseRoomSize);
        tilemap.GetComponent<Transform>().Position = bounds.ExactCenter * BaseRoomSize;
        
        Logger.Log($"Cave generation: generating rooms", stopwatch);
        GenerateRooms();
        SetupRoomCameraBounds();
        
        Logger.Log($"Cave generation: processing map", stopwatch);
        foreach (var (_, processor) in mapProcessors)
        {
            processor.ProcessMap(tilemap, CaveSystemManager.CurrentLevel);
        }
        
        Logger.Log($"Cave generation: baking light map", stopwatch);
        tilemap.GetComponent<LightMap>().ForceUpdate();
        Logger.Log($"Cave generation: light map baking completed", stopwatch);
        
        IsGenerating = false;
        
        Logger.Log($"Cave generation: finishing up", stopwatch);
        SpawnPlayer(playerController);

        Logger.Log($"Cave generation completed", stopwatch);
    }

    private void SpawnPlayer(PlayerController playerController)
    {
        foreach (Room room in CaveSystemManager.CurrentLevel.Rooms)
        {
            if ((room.Flags & RoomFlags.Entrance) != 0)
            {
                playerController.OwnerEntity.Position = RoomMath.RoomPosToTilemapPos(CaveSystemManager.CurrentLevel, room,
                    room.PointsOfInterest[PointOfInterestType.PlayerSpawnPoint][0].Position, BaseRoomSize);
                break;
            }
        }
    }

    private void SetupRoomCameraBounds()
    {
        cameraBoundsMap.Clear();

        foreach (CaveSystemLevel level in CaveSystemManager.CaveSystem.Levels)
        {
            foreach (Room room in level.Rooms)
            {
                CoordBounds bounds = RoomMath.GetRoomBounds(CaveSystemManager.CurrentLevel, room, BaseRoomSize);
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
            
            roomMapGenerator.GenerateRoomMap(room, parameters, RoomMath.TransformRoomPos(CaveSystemManager.CurrentLevel, room.Position, BaseRoomSize), CaveSystemManager.CaveSystem.BiomeMap, BaseRoomSize);
        }
    }

    public Room GetRoom(Coord tilemapPos)
    {
        foreach (Room room in CaveSystemManager.CurrentLevel.Rooms)
        {
            CoordBounds roomBounds = RoomMath.GetRoomBounds(CaveSystemManager.CurrentLevel, room, BaseRoomSize);
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
            Coord topLeft = RoomMath.TransformRoomPos(CaveSystemManager.CurrentLevel, room.Bounds.TopLeft, BaseRoomSize);
            Coord topRight = RoomMath.TransformRoomPos(CaveSystemManager.CurrentLevel, room.Bounds.TopRight + Coord.UnitX, BaseRoomSize);
            Coord bottomLeft = RoomMath.TransformRoomPos(CaveSystemManager.CurrentLevel, room.Bounds.BottomLeft + Coord.UnitY, BaseRoomSize);
            Coord bottomRight = RoomMath.TransformRoomPos(CaveSystemManager.CurrentLevel, room.Bounds.BottomRight + Coord.One, BaseRoomSize);

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
                Coord from = RoomMath.TransformRoomPos(CaveSystemManager.CurrentLevel, connection.From.Position, BaseRoomSize) + Coord.One * BaseRoomSize / 2;
                Coord to = RoomMath.TransformRoomPos(CaveSystemManager.CurrentLevel, connection.To.Position, BaseRoomSize) + Coord.One * BaseRoomSize / 2;
                Vector2 fromPos = tilemap.CoordToWorldPoint(from);
                Vector2 toPos = tilemap.CoordToWorldPoint(to);
                gizmos.DrawLine(fromPos + Vector2.One * 0.5f, toPos + Vector2.One * 0.5f, Color.Red, lifetime: 0);
            }
        }
    }
}