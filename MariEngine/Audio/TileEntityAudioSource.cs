using System;
using System.Collections.Generic;
using System.Linq;
using MariEngine.Components;
using MariEngine.Services;
using MariEngine.Tiles;
using Microsoft.Xna.Framework;

namespace MariEngine.Audio;

public class TileEntityAudioSource : TileEntityComponent<AudioSourceData>
{
    private PositionalAudioSource source = new();
    
    public TileEntityAudioSource WithEvent(string eventId, AudioEvent audioEvent)
    {
        source.WithEvent(eventId, audioEvent);
        return this;
    }

    public TileEntityAudioSource WithTrait(AudioTrait trait)
    {
        source.WithTrait(trait);
        return this;
    }
    
    private Vector2 GetWorldPos(Vector2 pos) => OwnerEntity.Tilemap.Vector2ToWorldPoint(pos);

    public void Play(string eventId) => source.Play(eventId);

    protected internal override void OnPositionUpdate()
    {
        source.SetPosition(GetWorldPos(OwnerEntity.SmoothedPosition));
        base.OnPositionUpdate();
    }

    protected override void OnDestroy()
    {
        source.Dispose();
        base.OnDestroy();
    }
    
    public override void Build(AudioSourceData data)
    {
        foreach (var (eventId, eventData) in data.Events) 
            WithEvent(eventId, ServiceRegistry.Get<AudioManager>().GetEvent(eventData.Path, eventData.OneShot));
        foreach (var trait in data.Traits)
        {
            var traitType = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.IsAssignableTo(typeof(AudioTrait)) && !t.IsAbstract);
            if (traitType is null)
                throw new Exception($"{trait} is not an audio trait type.");

            var traitObj = traitType.IsAssignableTo(typeof(WorldAudioTrait))
                ? (WorldAudioTrait)Activator.CreateInstance(traitType, [OwnerEntity.Tilemap])
                : (AudioTrait)Activator.CreateInstance(traitType);
            WithTrait(traitObj);
        }
    }
}