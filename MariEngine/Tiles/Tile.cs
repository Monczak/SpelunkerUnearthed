using System;
using System.Collections.Generic;
using MariEngine.Collision;
using MariEngine.Light;
using MariEngine.Services;
using MariEngine.Utils;
using Microsoft.Xna.Framework;

namespace MariEngine.Tiles;

public class Tile
{
    public string Id { get; }
    
    public Color ForegroundColor { get; set; }
    public Color BackgroundColor { get; set; }
    public char Character { get; set; }
    
    public LightSource LightSource { get; }
    public float LightAttenuation { get; }
    
    public HashSet<string> Tags { get; }
    
    public Tilemap OwnerTilemap { get; set; }
    public HashSet<TileBehavior> Behaviors { get; }

    public CollisionGroup CollisionGroup { get; }
    
    public Tile(string id, TileData data)
    {
        Id = id;
        ForegroundColor = ColorUtils.FromHex(data.ForegroundColor);
        BackgroundColor = ColorUtils.FromHex(data.BackgroundColor);
        Character = data.Character;

        Tags = data.Tags is null ? new HashSet<string>() : new HashSet<string>(data.Tags);

        CollisionGroup = CollisionGroup.None;
        if (data.CollisionGroups is not null)
        {
            foreach (string groupStr in data.CollisionGroups)
            {
                CollisionGroup group = (CollisionGroup)Enum.Parse(typeof(CollisionGroup), groupStr, true);
                CollisionGroup |= group;
            }
        }

        LightAttenuation = data.LightAttenuation;

        if (data.Light is not null)
        {
            LightSource = new PointLight(ColorUtils.FromHex(data.Light.Value.Color), data.Light.Value.Radius);
        }
        
        Behaviors = new HashSet<TileBehavior>();
    }

    public Tile(Tile tile)
    {
        Id = tile.Id;
        ForegroundColor = tile.ForegroundColor;
        BackgroundColor = tile.BackgroundColor;
        Character = tile.Character;
        
        Tags = tile.Tags is null ? new HashSet<string>() : new HashSet<string>(tile.Tags);

        if (tile.LightSource is not null)
            LightSource = tile.LightSource.Clone() as LightSource;
        LightAttenuation = tile.LightAttenuation;

        CollisionGroup = tile.CollisionGroup;

        Behaviors = new HashSet<TileBehavior>(tile.Behaviors);
    }

    public void OnPlaced()
    {
        foreach (var behavior in Behaviors)
            behavior.OnPlaced();
    }
    
    public void OnMined()
    {
        foreach (var behavior in Behaviors)
            behavior.OnMined();
    }
    
    public void OnSteppedOn(TileEntity steppingEntity)
    {
        foreach (var behavior in Behaviors)
            behavior.OnSteppedOn(steppingEntity);
    }
}