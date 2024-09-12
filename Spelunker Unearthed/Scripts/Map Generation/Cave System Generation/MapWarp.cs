using MariEngine;

namespace SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;

public readonly record struct MapWarp(
    int FromLevel,
    int ToLevel,
    Coord FromCoord,
    Coord ToCoord
);