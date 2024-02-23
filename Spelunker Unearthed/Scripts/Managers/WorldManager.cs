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

    public CaveSystemManager CaveSystemManager { get; } = caveSystemManager;

    private Dictionary<Room, CameraBounds> cameraBoundsMap = new();
    private int cameraBoundsOversize = 5;

    private SortedList<int, MapProcessor> mapProcessors = new();
    private SortedList<int, IRoomMapProcessor> roomMapProcessors = new();
    
    public int BaseRoomSize => 16;
    public int WorkerThreads { get; set; } = 8;
    
    public bool IsGenerating { get; private set; }

    public void AddProcessor(MapProcessor processor, int priority)
    {
        mapProcessors.Add(priority, processor);
    }
    
    public void AddRoomMapProcessor(IRoomMapProcessor processor, int priority)
    {
        roomMapProcessors.Add(priority, processor);
    }

    public void GenerateWorld()
    {
        CaveSystemManager.Generate();
    }

    public Task StartGenerateWorldTask()
    {
        if (IsGenerating) return null;
        return Task.Run(GenerateWorld).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Logger.LogError($"World generation failed: {task.Exception}");
            }
        });
    }

    // TODO: Load level from file instead of generating
    public Task<CaveSystemLevel> StartLoadLevelTask(int index)
    {
        if (IsGenerating) return null;
        return Task.Run(() =>
        {
            IsGenerating = true;
            var level = CaveSystemManager.CaveSystem.Levels[index];
            var (walls, ground) = GenerateCaveSystemLevel(level);
            
            // TODO: DEBUG - remove this
            using (var context = ServiceRegistry.Get<SaveLoadSystem>().LoadSaveFile("TestSave"))
            {
                context.Save(walls, "Walls");
                context.Save(ground, "Ground");
                context.Save(level, "Level");
            }
            
            using (var context = ServiceRegistry.Get<SaveLoadSystem>().LoadSaveFile("TestSave"))
            {
                walls = context.Load<TileBuffer>("Walls");
                ground = context.Load<TileBuffer>("Ground");
                level = context.Load<CaveSystemLevel>("Level");
            }
            
            LoadLevel(level, walls, ground);
            IsGenerating = false;

            return level;
        });
    }

    // TODO: Decouple tilemap from level generation - put everything into TileBuffers for walls and ground and then set the tilemap to use those buffers
    // This will make implementing a world save system way easier
    //
    // This needs to be refactored like so:
    // - Generate the cave system
    // - For each level in the system:
    //      - Generate the level (populate its tile buffers) 
    //      - Save the buffers to files (or a single file)
    //          - Devise a world file format (only tile IDs will need to be stored, probably, since tiles have no state - figure out compression)
    // 
    // Load cave system level in a separate method, taking in a parameter which specifies the level's index
    // Assign the loaded tile buffers to the tilemap and spawn the player where they should be (spawn point, ladder, etc.)
    // Get rid of CaveSystemManager.CurrentLevel?
    private (TileBuffer walls, TileBuffer ground) GenerateCaveSystemLevel(CaveSystemLevel level)
    {
        var stopwatch = Logger.StartStopwatch();
        Logger.Log($"Level generation started", stopwatch);

        CoordBounds bounds = level.BoundingBox;
        TileBuffer walls = new(bounds.Size * BaseRoomSize);
        TileBuffer ground = new(bounds.Size * BaseRoomSize);
        
        Logger.Log($"Level generation: generating rooms", stopwatch);
        GenerateRooms(walls, ground, level);
        
        Logger.Log($"Level generation: processing map", stopwatch);
        foreach (var (_, processor) in mapProcessors)
        {
            processor.ProcessMap(walls, level);
        }
        
        Logger.Log($"Level generation completed", stopwatch);
        
        return (walls, ground);
    }

    public void LoadLevel(CaveSystemLevel level, TileBuffer walls, TileBuffer ground)
    {
        var stopwatch = Logger.StartStopwatch();
        
        Logger.Log($"Loading level", stopwatch);
        tilemap.Resize(new Coord(walls.Width, walls.Height));
        tilemap.GetComponent<Transform>().Position = level.BoundingBox.ExactCenter * BaseRoomSize;

        Parallel.ForEach(walls.Coords, coord =>
        {
            tilemap.Place(walls[coord], coord, Tilemap.BaseLayer);
            tilemap.Place(ground[coord], coord, Tilemap.GroundLayer);
        });
        
        SetupRoomCameraBounds(level);
        
        Logger.Log($"Loading level: baking light map", stopwatch);
        tilemap.GetComponent<LightMap>().ForceUpdate();
        Logger.Log($"Loading level: light map baking completed", stopwatch);
        
        Logger.Log($"Loading level: finishing up", stopwatch);
        SpawnPlayer(level, playerController);

        Logger.Log($"Level loading completed", stopwatch);
    }

    // TODO: Create a player instead of manipulating an existing player
    private void SpawnPlayer(CaveSystemLevel level, PlayerController playerController)
    {
        foreach (Room room in level.Rooms)
        {
            if ((room.Flags & RoomFlags.Entrance) != 0)
            {
                playerController.OwnerEntity.Position = RoomMath.RoomPosToTilemapPos(level, room,
                    room.PointsOfInterest[PointOfInterestType.PlayerSpawnPoint][0].Position, BaseRoomSize);
                break;
            }
        }
    }

    private void SetupRoomCameraBounds(CaveSystemLevel level)
    {
        cameraBoundsMap.Clear();
        
        foreach (Room room in level.Rooms)
        {
            CoordBounds bounds = RoomMath.GetRoomBounds(level, room, BaseRoomSize);
            Bounds worldBounds = Bounds.MakeCorners(tilemap.CoordToWorldPoint(bounds.TopLeft) - Vector2.One * cameraBoundsOversize,
                tilemap.CoordToWorldPoint(bounds.BottomRight) + Vector2.One * cameraBoundsOversize);
            cameraBoundsMap[room] = new CameraBounds(worldBounds);
        }
    }

    private void GenerateRooms(TileBuffer walls, TileBuffer ground, CaveSystemLevel level)
    {
        Parallel.ForEach(walls.Coords, coord =>
        {
            walls[coord] = CaveSystemManager.CaveSystem.BiomeMap.GetWall(coord);
            ground[coord] = CaveSystemManager.CaveSystem.BiomeMap.GetGround(coord);
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

            var pastePosition = RoomMath.TransformRoomPos(level, room.Position, BaseRoomSize);
            var (roomWalls, roomGround) = new RoomMapGenerator(roomMapProcessors.Values)
                .GenerateRoomMap(room, parameters, pastePosition, CaveSystemManager.CaveSystem.BiomeMap, BaseRoomSize);

            walls.PasteAt(roomWalls, pastePosition);
            ground.PasteAt(roomGround, pastePosition);
        });
    }

    public Room GetRoom(CaveSystemLevel level, Coord tilemapPos)
    {
        foreach (Room room in level.Rooms)
        {
            CoordBounds roomBounds = RoomMath.GetRoomBounds(level, room, BaseRoomSize);
            if (roomBounds.PointInside(tilemapPos))
                return room;
        }

        return null;
    }
    
    public CameraBounds GetRoomCameraBounds(CaveSystemLevel level, Coord tilemapPos)
    {
        Room room = GetRoom(level, tilemapPos);
        if (room is null) return null;
        return cameraBoundsMap[room];
    }
    
    // public void DrawLevel(int level)
    // {
    //     foreach (Room room in CaveSystemManager.CaveSystem.Levels[level].Rooms)
    //     {
    //         Coord topLeft = RoomMath.TransformRoomPos(CaveSystemManager.CurrentLevel, room.Bounds.TopLeft, BaseRoomSize);
    //         Coord topRight = RoomMath.TransformRoomPos(CaveSystemManager.CurrentLevel, room.Bounds.TopRight + Coord.UnitX, BaseRoomSize);
    //         Coord bottomLeft = RoomMath.TransformRoomPos(CaveSystemManager.CurrentLevel, room.Bounds.BottomLeft + Coord.UnitY, BaseRoomSize);
    //         Coord bottomRight = RoomMath.TransformRoomPos(CaveSystemManager.CurrentLevel, room.Bounds.BottomRight + Coord.One, BaseRoomSize);
    //
    //         Vector2 topLeftV = tilemap.CoordToWorldPoint(topLeft) + Vector2.One * 0.1f;
    //         Vector2 topRightV = tilemap.CoordToWorldPoint(topRight) + (-Vector2.UnitX + Vector2.UnitY) * 0.1f;
    //         Vector2 bottomLeftV = tilemap.CoordToWorldPoint(bottomLeft) + (Vector2.UnitX + -Vector2.UnitY)* 0.1f;
    //         Vector2 bottomRightV = tilemap.CoordToWorldPoint(bottomRight) + -Vector2.One * 0.1f;
    //         
    //         gizmos.DrawLine(topLeftV, topRightV, Color.Blue, lifetime: 0);
    //         gizmos.DrawLine(topRightV, bottomRightV, Color.Blue, lifetime: 0);
    //         gizmos.DrawLine(bottomRightV, bottomLeftV, Color.Blue, lifetime: 0);
    //         gizmos.DrawLine(bottomLeftV, topLeftV, Color.Blue, lifetime: 0);
    //         
    //         // gizmos.DrawRectangle((Vector2)room.Bounds.TopLeft + Vector2.One * 0.05f, (Vector2)room.Bounds.Size - Vector2.One * 0.1f,
    //         //     new Color(0, MathUtils.InverseLerp(20, 0, room.Distance), MathUtils.InverseLerp(20, 0, room.Distance), 0.1f), 0);
    //         foreach (SubRoomConnection connection in room.Connections)
    //         {
    //             Coord from = RoomMath.TransformRoomPos(CaveSystemManager.CurrentLevel, connection.From.Position, BaseRoomSize) + Coord.One * BaseRoomSize / 2;
    //             Coord to = RoomMath.TransformRoomPos(CaveSystemManager.CurrentLevel, connection.To.Position, BaseRoomSize) + Coord.One * BaseRoomSize / 2;
    //             Vector2 fromPos = tilemap.CoordToWorldPoint(from);
    //             Vector2 toPos = tilemap.CoordToWorldPoint(to);
    //             gizmos.DrawLine(fromPos + Vector2.One * 0.5f, toPos + Vector2.One * 0.5f, Color.Red, lifetime: 0);
    //         }
    //     }
    // }
}