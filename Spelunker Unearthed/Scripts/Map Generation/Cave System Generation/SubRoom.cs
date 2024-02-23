using System;
using MariEngine;
using Microsoft.Xna.Framework;

namespace SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;

public class SubRoom
{
    // ReSharper disable once UnusedMember.Global (YAML serialization)
    public SubRoom()
    {
    }

    public SubRoom(Room room, Coord position)
    {
        Position = position;
        Room = room;
    }

    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local (required for YAML serialization)
    public Coord Position { get; private set; }
    
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local (required for YAML serialization)
    public Room Room { get; private set; }

    public bool NextTo(SubRoom subRoom) => (subRoom.Position - Position).SqrMagnitude == 1;
}