using MariEngine;

namespace SpelunkerUnearthed.Scripts.MapGeneration;

public struct PointOfInterest
{
    public PointOfInterestType PoiType { get; init; }
    public Coord Position { get; init; }

    public PointOfInterest(PointOfInterestType poiType, Coord position)
    {
        PoiType = poiType;
        Position = position;
    }
}