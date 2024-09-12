using MariEngine;

namespace SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;

public record MapWarp(
    int FromLevel,
    int ToLevel,
    Coord FromCoord,
    Coord ToCoord
);