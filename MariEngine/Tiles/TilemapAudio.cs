using MariEngine.Audio;
using MariEngine.Components;
using MariEngine.Services;
using Microsoft.Xna.Framework;

namespace MariEngine.Tiles;

public class TilemapAudio(PositionalAudioSource source) : Component
{
    private Tilemap tilemap;

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
}