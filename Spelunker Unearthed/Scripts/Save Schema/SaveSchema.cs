using MariEngine.Persistence;

namespace SpelunkerUnearthed.Scripts.SaveSchema;

public static class Save
{
    public static World World { get; }

    static Save()
    {
        World = new World(null);
    }
}

public class World : PathElement
{
    public Levels Levels { get; }
    public CaveSystem CaveSystem { get; }

    public World(PathElement parent) : base("World", parent)
    {
        Levels = new Levels(this);
        CaveSystem = new CaveSystem(this);
    }
}

public class CaveSystem : PathElement
{
    public CaveSystem(PathElement parent) : base("CaveSystem", parent)
    {
        
    } 
}

public class Levels : PathElement
{
    public Level Level(int index) => new(index, this);
    public Level Level(string name) => new(name, this);
    
    public Levels(PathElement parent) : base("Levels", parent)
    {
        
    }
}

public class Level : PathElement
{
    public Walls Walls { get; }
    public Ground Ground { get; }
    public LevelData LevelData { get; }
    
    public Level(int index, PathElement parent) : this($"Level{index}", parent)
    {
    }

    public Level(string name, PathElement parent) : base(name, parent)
    {
        Walls = new Walls(this);
        Ground = new Ground(this);
        LevelData = new LevelData(this);
    }
}

public class Walls : PathElement
{
    public Walls(PathElement parent) : base("Walls", parent)
    {
        
    }
}

public class Ground : PathElement
{
    public Ground(PathElement parent) : base("Ground", parent)
    {
        
    }
}

public class LevelData : PathElement
{
    public LevelData(PathElement parent) : base("LevelData", parent)
    {
        
    }
}