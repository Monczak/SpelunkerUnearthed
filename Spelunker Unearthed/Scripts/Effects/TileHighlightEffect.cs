using System;
using MariEngine;
using MariEngine.Rendering;
using MariEngine.Tiles;
using MariEngine.Utils;
using Microsoft.Xna.Framework;

namespace SpelunkerUnearthed.Scripts.Effects;

public class TileHighlightEffect : TilemapRendererEffect
{
    public Coord? HighlightPosition { get; set; }
    private Coord? previousHighlightPosition;

    private float Frequency { get; set; } = 1.3f;
    private float MinHighlight { get; set; } = 0.1f;
    private float MaxHighlight { get; set; } = 0.4f;

    public override TilemapLayer LayerMask => TilemapLayer.Base;
    public override bool ApplyToTileEntities => false;
    
    private double lastPosUpdateTime;

    public override Color Apply(Color input, Coord worldPos, GameTime gameTime)
    {
        if (HighlightPosition != previousHighlightPosition)
        {
            lastPosUpdateTime = gameTime.TotalGameTime.TotalSeconds;
            previousHighlightPosition = HighlightPosition;
        }
        
        if (worldPos == HighlightPosition)
        {
            var highlightStrength = MathUtils.Lerp(MinHighlight, MaxHighlight,
                (MathF.Cos((float)(gameTime.TotalGameTime.TotalSeconds - lastPosUpdateTime) * MathF.Tau * Frequency) + 1) / 2);
            return Color.Lerp(input, Color.White, highlightStrength);
        }
        return input;
    }
}