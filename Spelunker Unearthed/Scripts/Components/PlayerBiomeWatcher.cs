﻿using System;
using MariEngine.Components;
using MariEngine.Events;
using MariEngine.Services;
using Microsoft.Xna.Framework;
using SpelunkerUnearthed.Scripts.Managers;
using SpelunkerUnearthed.Scripts.MapGeneration.Biomes;

namespace SpelunkerUnearthed.Scripts.Components;

public class PlayerBiomeWatcher(WorldManager worldManager, AmbienceController ambienceController) : TileEntityComponent
{
    private Biome currentBiome, prevBiome;

    protected override void OnPositionUpdate()
    {
        currentBiome = worldManager.CaveSystemManager.CaveSystem.BiomeMap.GetBiome(OwnerEntity.Position);
        if (currentBiome != prevBiome) ambienceController.SetBiomeAmbience(currentBiome);

        prevBiome = currentBiome;
    }
}