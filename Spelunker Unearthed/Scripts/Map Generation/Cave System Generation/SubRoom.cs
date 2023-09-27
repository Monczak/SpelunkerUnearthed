using System;
using MariEngine;
using Microsoft.Xna.Framework;

namespace SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;

public class SubRoom
{
    public Coord Position { get; private set; }
    public Room Room { get; private set; }

    public SubRoom(Room room, Coord position)
    {
        Room = room;
        Position = position;
    }

    public bool NextTo(SubRoom subRoom) => (subRoom.Position - Position).SqrMagnitude == 1;
}