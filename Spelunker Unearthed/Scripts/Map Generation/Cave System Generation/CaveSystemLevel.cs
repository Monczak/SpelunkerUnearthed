using System.Collections.Generic;
using MariEngine;

namespace SpelunkerUnearthed.Scripts.MapGeneration.CaveSystemGeneration;

public class CaveSystemLevel
{
    private List<Room> rooms = new();

    private Dictionary<Coord, Room> map = new();

    public void Build()
    {
        
    }

    private void AddRoom(Room room)
    {
        rooms.Add(room);

        foreach (Coord coord in room.RoomCoords)
            map[coord] = room;
    }

    private bool OverlapsRoom(Room room) => OverlapsRoom(room.Bounds);

    private bool OverlapsRoom(CoordBounds bounds)
    {
        foreach (Coord coord in bounds.Coords)
        {
            if (map.ContainsKey(coord))
                return true;
        }

        return false;
    }
}