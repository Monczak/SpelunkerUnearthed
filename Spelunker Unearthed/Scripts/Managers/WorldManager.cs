using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
using SpelunkerUnearthed.Scripts.MapGeneration.Features;
using SpelunkerUnearthed.Scripts.MapGeneration.MapProcessors;
using SpelunkerUnearthed.Scripts.SaveSchema;
using SpelunkerUnearthed.Scripts.TileEntities;
using SpelunkerUnearthed.Scripts.Utils;
using CaveSystem = SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration.CaveSystem;

namespace SpelunkerUnearthed.Scripts.Managers;

public class WorldManager(CaveSystemManager caveSystemManager, Tilemap tilemap, PlayerController playerController,
        Gizmos gizmos)
    : Component
{
    public CaveSystemManager CaveSystemManager { get; } = caveSystemManager;

    private Dictionary<Room, CameraBounds> cameraBoundsMap = new();
    private int cameraBoundsOversize = 5;

    private SortedList<int, MapProcessor> mapProcessors = new();
    private SortedList<int, IRoomMapProcessor> roomMapProcessors = new();
    
    public int BaseRoomSize => 16;
    public int WorkerThreads { get; set; } = 8;
    
    public bool IsGenerating { get; private set; }

    public WorldManager AddMapProcessor(MapProcessor processor, int priority)
    {
        mapProcessors.Add(-priority, processor);
        return this;
    }

    public WorldManager AddMapProcessor<T>(int priority) where T : MapProcessor
    {
        AddMapProcessor(Activator.CreateInstance<T>(), priority);
        return this;
    }
    
    public WorldManager AddRoomMapProcessor(IRoomMapProcessor processor, int priority)
    {
        roomMapProcessors.Add(-priority, processor);
        return this;
    }
    
    public WorldManager AddRoomMapProcessor<T>(int priority) where T : IRoomMapProcessor
    {
        AddRoomMapProcessor(Activator.CreateInstance<T>(), priority);
        return this;
    }

    public void GenerateWorld(int worldSeed)
    {
        CaveSystemManager.Generate(worldSeed, BaseRoomSize);
    }

    public Task StartGenerateWorldTask(int worldSeed)
    {
        if (IsGenerating) return null;
        return Task.Run(() => GenerateWorld(worldSeed)).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Logger.LogError($"World generation failed: {task.Exception}");
            }
            
            using var context = ServiceRegistry.Get<SaveLoadSystem>().LoadSaveFile("TestSave");
            context.Save(CaveSystemManager.CaveSystem, Save.World.CaveSystem);

            foreach (CaveSystemLevel level in CaveSystemManager.CaveSystem.Levels)
            {
                ServiceRegistry.Get<RandomProvider>().RequestPositionBased(Constants.MapGenRng).Seed(level.MapGenSeed);
                
                var (walls, ground) = GenerateCaveSystemLevel(level);
                
                context.Save(walls, Save.World.Levels.Level(level.Depth).Walls);
                context.Save(ground, Save.World.Levels.Level(level.Depth).Ground);
                context.Save(level, Save.World.Levels.Level(level.Depth).LevelData);
            }
        });
    }

    public void LoadWorld()
    {
        using var context = ServiceRegistry.Get<SaveLoadSystem>().LoadSaveFile("TestSave");
        CaveSystemManager.Load(context.Load<CaveSystem>(Save.World.CaveSystem));
        var levels = context.GetHierarchy(Save.World.Levels)
            .Select(levelName => context.Load<CaveSystemLevel>(Save.World.Levels.Level(levelName).LevelData))
            .ToList();
        
        ServiceRegistry.Get<RandomProvider>().Request(Constants.BiomeGenRng).Seed(CaveSystemManager.CaveSystem.Seed);

        CaveSystemManager.CaveSystem.Levels = levels;
    }

    public Task StartLoadLevelTask(CaveSystemLevel level)
    {
        if (IsGenerating)
        {
            Logger.LogError("Trying to load level, but IsGenerating is set");
            return null;
        }
        return Task.Run(() =>
        {
            IsGenerating = true;

            TileBuffer walls, ground;
            using (var context = ServiceRegistry.Get<SaveLoadSystem>().LoadSaveFile("TestSave"))
            {
                walls = context.Load<TileBuffer>(Save.World.Levels.Level(level.Depth).Walls);
                ground = context.Load<TileBuffer>(Save.World.Levels.Level(level.Depth).Ground);
            }

            caveSystemManager.SetCurrentLevel(level);
            
            LoadLevel(level, walls, ground);
            IsGenerating = false;

            return level;
        })
        .ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Logger.LogError($"Loading level {level.Depth} failed: {task.Exception}");
            }
        });
    }

    private (TileBuffer walls, TileBuffer ground) GenerateCaveSystemLevel(CaveSystemLevel level)
    {
        var stopwatch = Logger.StartStopwatch();
        Logger.Log($"Level generation started", stopwatch);

        CoordBounds bounds = level.BoundingBox;
        TileBuffer walls = new(bounds.Size * level.BaseRoomSize);
        TileBuffer ground = new(bounds.Size * level.BaseRoomSize);
        
        Logger.Log($"Level generation: generating rooms", stopwatch);
        GenerateRooms(walls, ground, level);
        
        Logger.Log($"Level generation: processing map", stopwatch);
        foreach (var (_, processor) in mapProcessors)
        {
            processor.ProcessMap(walls, ground, level);
        }
        
        Logger.Log($"Level generation completed", stopwatch);
        
        return (walls, ground);
    }

    public void LoadLevel(CaveSystemLevel level, TileBuffer walls, TileBuffer ground)
    {
        var stopwatch = Logger.StartStopwatch();
        
        Logger.Log($"Loading level", stopwatch);
        tilemap.Resize(new Coord(walls.Width, walls.Height));
        tilemap.GetComponent<Transform>().Position = level.BoundingBox.ExactCenter * level.BaseRoomSize;

        Logger.Log($"Loading level: copying tile buffers", stopwatch);
        Parallel.ForEach(walls.Coords, coord =>
        {
            tilemap.Place(walls[coord], coord, TilemapLayer.Base);
            tilemap.Place(ground[coord], coord, TilemapLayer.Ground);
        });
        
        SetupRoomCameraBounds(level);
        
        Logger.Log($"Loading level: baking light map", stopwatch);
        tilemap.GetComponent<LightMap>().ForceUpdate();
        Logger.Log($"Loading level: light map baking completed", stopwatch);
        
        Logger.Log($"Loading level: finishing up", stopwatch);
        SpawnPlayer(level, playerController);

        Logger.Log($"Level loading completed", stopwatch);
    }
    
    private void SpawnPlayer(CaveSystemLevel level, PlayerController playerController)
    {
        foreach (Room room in level.Rooms)
        {
            if ((room.Flags & RoomFlags.Entrance) != 0)
            {
                playerController.OwnerEntity.Position = RoomMath.RoomPosToTilemapPos(level, room,
                    room.PointsOfInterest[PointOfInterestType.PlayerSpawnPoint][0].Position);
                break;
            }
        }
    }

    private void SetupRoomCameraBounds(CaveSystemLevel level)
    {
        cameraBoundsMap.Clear();
        
        foreach (Room room in level.Rooms)
        {
            CoordBounds bounds = RoomMath.GetRoomBounds(level, room);
            Bounds worldBounds = Bounds.MakeCorners(tilemap.CoordToWorldPoint(bounds.TopLeft) - Vector2.One * cameraBoundsOversize,
                tilemap.CoordToWorldPoint(bounds.BottomRight) + Vector2.One * cameraBoundsOversize);
            cameraBoundsMap[room] = new CameraBounds(worldBounds);
        }
    }

    private void GenerateRooms(TileBuffer walls, TileBuffer ground, CaveSystemLevel level)
    {
        Parallel.ForEach(walls.Coords, coord =>
        {
            walls[coord] = CaveSystemManager.CaveSystem.BiomeMap.GetWall(coord, level.Depth);
            ground[coord] = CaveSystemManager.CaveSystem.BiomeMap.GetGround(coord, level.Depth);
        });

        Parallel.ForEach(level.Rooms, room =>
        {
            RoomMapGenerationParameters parameters = new RoomMapGenerationParameters
            {
                NothingTile = ServiceRegistry.Get<TileLoader>().Get("Nothing"),
                BorderSize = 1,
                BorderGradientSize = 2,
                BorderGradientFillAmount = 0.6f,
            };

            var pastePosition = RoomMath.TransformRoomPos(level, room.Position);
            var (roomWalls, roomGround) = new RoomMapGenerator(
                    roomMapProcessors.Values,
                    level.Depth,
                    room,
                    CaveSystemManager.CaveSystem.BiomeMap,
                    parameters,
                    level.BaseRoomSize
                )
                .GenerateRoomMap(pastePosition);

            walls.PasteAt(roomWalls, pastePosition);
            ground.PasteAt(roomGround, pastePosition);
        });
    }

    public Room GetRoom(CaveSystemLevel level, Coord tilemapPos)
    {
        foreach (Room room in level.Rooms)
        {
            CoordBounds roomBounds = RoomMath.GetRoomBounds(level, room);
            if (roomBounds.PointInside(tilemapPos))
                return room;
        }

        return null;
    }
    
    public CameraBounds GetRoomCameraBounds(CaveSystemLevel level, Coord tilemapPos)
    {
        if (level is null) return null;
        Room room = GetRoom(level, tilemapPos);
        if (room is null) return null;
        return cameraBoundsMap[room];
    }
    
    public void DrawLevel(CaveSystemLevel level)
    {
        if (level is null) return;
        
        foreach (Room room in level.Rooms)
        {
            Coord topLeft = RoomMath.TransformRoomPos(CaveSystemManager.CurrentLevel, room.Bounds.TopLeft);
            Coord topRight = RoomMath.TransformRoomPos(CaveSystemManager.CurrentLevel, room.Bounds.TopRight + Coord.UnitX);
            Coord bottomLeft = RoomMath.TransformRoomPos(CaveSystemManager.CurrentLevel, room.Bounds.BottomLeft + Coord.UnitY);
            Coord bottomRight = RoomMath.TransformRoomPos(CaveSystemManager.CurrentLevel, room.Bounds.BottomRight + Coord.One);
    
            Vector2 topLeftV = tilemap.CoordToWorldPoint(topLeft) + Vector2.One * 0.1f;
            Vector2 topRightV = tilemap.CoordToWorldPoint(topRight) + (-Vector2.UnitX + Vector2.UnitY) * 0.1f;
            Vector2 bottomLeftV = tilemap.CoordToWorldPoint(bottomLeft) + (Vector2.UnitX + -Vector2.UnitY)* 0.1f;
            Vector2 bottomRightV = tilemap.CoordToWorldPoint(bottomRight) + -Vector2.One * 0.1f;
            
            gizmos.DrawLine(topLeftV, topRightV, Color.Blue, lifetime: 0);
            gizmos.DrawLine(topRightV, bottomRightV, Color.Blue, lifetime: 0);
            gizmos.DrawLine(bottomRightV, bottomLeftV, Color.Blue, lifetime: 0);
            gizmos.DrawLine(bottomLeftV, topLeftV, Color.Blue, lifetime: 0);
            
            gizmos.DrawRectangle((Vector2)room.Bounds.TopLeft + Vector2.One * 0.05f, (Vector2)room.Bounds.Size - Vector2.One * 0.1f,
                new Color((room.Flags & RoomFlags.LadderRoom) != 0 ? 255 : 0, MathUtils.InverseLerp(20, 0, room.Distance), MathUtils.InverseLerp(20, 0, room.Distance), 0.1f), 0);
            foreach (SubRoomConnection connection in room.Connections)
            {
                Coord from = RoomMath.TransformRoomPos(level, connection.From.Position) + Coord.One * level.BaseRoomSize / 2;
                Coord to = RoomMath.TransformRoomPos(level, connection.To.Position) + Coord.One * level.BaseRoomSize / 2;
                Vector2 fromPos = tilemap.CoordToWorldPoint(from);
                Vector2 toPos = tilemap.CoordToWorldPoint(to);
                gizmos.DrawLine(fromPos + Vector2.One * 0.5f, toPos + Vector2.One * 0.5f, Color.Red, lifetime: 0);
            }
        }
    }
}