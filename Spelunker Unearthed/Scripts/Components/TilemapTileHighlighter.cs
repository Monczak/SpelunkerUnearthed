using MariEngine;
using MariEngine.Components;
using MariEngine.Events;
using MariEngine.Rendering;
using MariEngine.Services;
using MariEngine.Tiles;
using Microsoft.Xna.Framework;
using SpelunkerUnearthed.Scripts.Effects;
using SpelunkerUnearthed.Scripts.TileEntities;

namespace SpelunkerUnearthed.Scripts.Components;

public class TilemapTileHighlighter(TileHighlightEffect effect) : Component
{
    public Coord? HighlightedCoord { get; set; } = null;

    protected override void Initialize()
    {
        base.Initialize();
        
        ServiceRegistry.Get<EventManager>().Bind(this, "PlayerMoved", OnPlayerMoved);
    }

    private void OnPlayerMoved(PlayerController player)
    {
        HighlightedCoord = player.OwnerEntity.Position + player.FacingDirection;
    }
    
    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        effect.HighlightPosition = HighlightedCoord;
    }
}