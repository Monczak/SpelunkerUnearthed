using System.Collections.Generic;
using System.IO;
using MariEngine.Logging;
using MariEngine.Persistence;
using MariEngine.Services;
using SpelunkerUnearthed.Scripts.MapGeneration.Biomes;
using SpelunkerUnearthed.Scripts.MapGeneration.MapProcessors;
using YamlDotNet.Serialization;

namespace SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;

public class CaveSystem(IBiomeProvider biomeProvider, RoomDecisionEngine roomDecisionEngine, IEnumerable<IRoomLayoutProcessor> roomLayoutProcessors) : IYamlSaveable<CaveSystem>
{
    public int Seed { get; private set; }
    
    [YamlIgnore] public List<CaveSystemLevel> Levels { get; set; } = [];
    [YamlIgnore] public BiomeMap BiomeMap { get; private set; } = new(biomeProvider);

    private const int MaxGenerationAttempts = 10;

    // Required for YAML serialization
    // TODO: Either save provider names (to be loaded later with reflection), or load defaults from a defaults class (not like this)
    public CaveSystem() : this(new SimpleBiomeProvider(), new TestDecisionEngine(),
        new List<IRoomLayoutProcessor> { new LadderRoomProcessor() })
    {
        
    }
    
    public void Generate(int seed)
    {
        Seed = seed;
        ServiceRegistry.Get<RandomProvider>().Request(Constants.CaveSystemGenRng).Seed(seed);
        ServiceRegistry.Get<RandomProvider>().Request(Constants.BiomeGenRng).Seed(seed);
        
        Levels.Clear();
        
        // TODO: Add levels procedurally
        Levels.Add(new CaveSystemLevel { Depth = 0, MapGenSeed = ServiceRegistry.Get<RandomProvider>().Request(Constants.CaveSystemGenRng).Next()});
        
        for (int i = 0; i < Levels.Count; i++)
        {
            var level = Levels[i];
            int attempt = 0;
            while (attempt < MaxGenerationAttempts)
            {
                level.Generate(roomDecisionEngine, roomLayoutProcessors);
                attempt++;

                if (!roomDecisionEngine.ShouldRegenerate(level))
                    break;
            }

            if (attempt == MaxGenerationAttempts)
                Logger.LogWarning($"Exceeded max generation attempts for level {i}");
        }
    }

    public void Serialize(Stream stream)
    {
        var writer = new StreamWriter(stream);
        writer.Write(new SerializerBuilder()
            .Build()
            .Serialize(this)
        );
        writer.Flush();
    }

    public static CaveSystem Deserialize(Stream stream)
    {
        var reader = new StreamReader(stream);
        var caveSystem = new DeserializerBuilder()
            .Build()
            .Deserialize<CaveSystem>(reader.ReadToEnd());
        return caveSystem;
    }
}