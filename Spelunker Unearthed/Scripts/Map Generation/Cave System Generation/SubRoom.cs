using System;
using MariEngine;
using Microsoft.Xna.Framework;

namespace SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;

public class SubRoom(Room room, Coord position)
{
    public Coord Position { get; private set; } = position;
    public Room Room { get; private set; } = room;

    public bool NextTo(SubRoom subRoom) => (subRoom.Position - Position).SqrMagnitude == 1;
}