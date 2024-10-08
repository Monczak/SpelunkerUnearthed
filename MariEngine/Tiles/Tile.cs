﻿using System;
using System.Collections.Generic;
using System.Linq;
using MariEngine.Collision;
using MariEngine.Exceptions;
using MariEngine.Light;
using MariEngine.Loading;
using MariEngine.Services;
using MariEngine.Utils;
using Microsoft.Xna.Framework;

namespace MariEngine.Tiles;

public class Tile : Resource<TileData>
{
    public Color ForegroundColor { get; set; }
    public Color BackgroundColor { get; set; }
    public char Character { get; set; }
    
    public LightSource LightSource { get; private set; }
    public float LightAttenuation { get; private set; }
    
    public int Tags { get; private set; }

    public HashSet<TileBehavior> Behaviors { get; private set; }

    public CollisionGroup CollisionGroup { get; private set; }
    
    public TileType Type { get; private set; }
    
    public Material Material { get; private set; }

    public Tile()
    {
        
    }

    public void OnPlaced(Coord position)
    {
        foreach (var behavior in Behaviors)
            behavior.OnPlaced(this, position);
    }
    
    public void OnMined(Coord position)
    {
        foreach (var behavior in Behaviors)
            behavior.OnMined(this, position);
    }
    
    public void OnSteppedOn(Coord position, TileEntity steppingEntity)
    {
        foreach (var behavior in Behaviors)
            behavior.OnSteppedOn(this, position, steppingEntity);
    }

    protected internal override void BuildFromData(TileData data)
    {
        ForegroundColor = ColorUtils.FromHex(data.ForegroundColor);
        ForegroundColor = new Color(ForegroundColor.R, ForegroundColor.G, ForegroundColor.B, (byte)255);
        BackgroundColor = ColorUtils.FromHex(data.BackgroundColor);
        BackgroundColor = new Color(BackgroundColor.R, BackgroundColor.G, BackgroundColor.B, (byte)255);
        Character = data.Character;

        if (data.Tags is not null)
        {
            foreach (string tag in data.Tags)
            {
                Tags |= MariEngine.Tags.GetValue(tag);
            }
        }

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
            LightSource = new PointLight(ColorUtils.FromHex(data.Light.Value.Color), data.Light.Value.Intensity, data.Light.Value.Radius);
        }

        if (data.Type is null)
        {
            Type = TileType.World;
        }
        else
        {
            Type = Enum.Parse<TileType>(data.Type);
        }

        if (data.Material is null)
        {
            Material = ServiceRegistry.Get<MaterialLoader>().Get("None");
        }
        else
        {
            Material = ServiceRegistry.Get<MaterialLoader>().Get(data.Material);
        }
        
        Behaviors = [];
        if (data.Behaviors is not null)
        {
            foreach (var behaviorName in data.Behaviors)
            {
                if (Behaviors.Any(b => b.GetType().Name == behaviorName))
                    throw new ContentLoadingException($"Found duplicate behavior {behaviorName}");
            
                var behavior = ServiceRegistry.Get<TileBehaviorProvider>().CreateBehavior(behaviorName);
                Behaviors.Add(behavior);
            }   
        }
    }
}