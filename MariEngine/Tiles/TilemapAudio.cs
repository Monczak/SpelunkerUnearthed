using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MariEngine.Audio;
using MariEngine.Components;
using MariEngine.Loading;
using MariEngine.Services;
using Microsoft.Xna.Framework;

namespace MariEngine.Tiles;

public class TilemapAudio : Component<AudioSourceData>
{
    private Tilemap tilemap;
    private PositionalAudioSource source = new();

    public TilemapAudio WithEvent(string eventId, AudioEvent audioEvent)
    {
        source.WithEvent(eventId, audioEvent);
        return this;
    }

    public TilemapAudio WithTrait(AudioTrait trait)
    {
        source.WithTrait(trait);
        return this;
    }

    protected internal override void Initialize()
    {
        tilemap = GetComponent<Tilemap>();
        tilemap.TileMined += OnTileMined;
    }

    private void OnTileMined(Coord position, Tile tile)
    {
        source.Play("Mine", tilemap.CoordToWorldPoint(position));
    }

    protected override void OnDestroy()
    {
        source.Dispose();
        base.OnDestroy();
    }
    
    public override void Build(AudioSourceData sourceData)
    {
        tilemap = OwnerEntity.GetComponent<Tilemap>();
        foreach (var (eventId, eventData) in sourceData.Events) 
            WithEvent(eventId, ServiceRegistry.Get<AudioManager>().GetEvent(eventData.Path, eventData.OneShot));
        foreach (var trait in sourceData.Traits)
        {
            var traitType = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.IsAssignableTo(typeof(AudioTrait)) && !t.IsAbstract);
            if (traitType is null)
                throw new Exception($"{trait} is not an audio trait type.");

            var traitObj = traitType.IsAssignableTo(typeof(WorldAudioTrait))
                ? (WorldAudioTrait)Activator.CreateInstance(traitType, [tilemap])
                : (AudioTrait)Activator.CreateInstance(traitType);
            WithTrait(traitObj);
        }
    }
}

public class AudioSourceData : ComponentData
{
    public class EventData
    {
        public string Path { get; init; }
        public bool OneShot { get; init; }
    }
    
    public List<string> Traits { get; init; }
    
    public Dictionary<string, EventData> Events { get; init; }
}