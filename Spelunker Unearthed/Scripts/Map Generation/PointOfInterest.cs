using MariEngine;

namespace SpelunkerUnearthed.Scripts.MapGeneration;

public readonly struct PointOfInterest(PointOfInterestType poiType, Coord position)
{
    public PointOfInterestType PoiType { get; } = poiType;
    public Coord Position { get; } = position;
}