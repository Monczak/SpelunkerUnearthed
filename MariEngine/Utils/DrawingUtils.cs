using System;
using System.Collections.Generic;

namespace MariEngine.Utils;

public static class DrawingUtils
{
    private static readonly List<Coord> Coords = new();
    
    public static void BresenhamLine(List<Coord> coordList, Coord start, Coord end, bool endPreemptively = false)
    {
        int dx = Math.Abs(end.X - start.X);
        int dy = Math.Abs(end.Y - start.Y);
        
        coordList.Clear();

        Coord step = new(start.X < end.X ? 1 : -1, start.Y < end.Y ? 1 : -1);

        Coord pos = start;
        int error = dx - dy;

        while (true)
        {
            if (pos == end && endPreemptively)
                break;

            coordList.Add(pos);

            if (pos == end)
                break;

            int error2 = error * 2;
            if (error2 > -dy)
            {
                error -= dy;
                pos.X += step.X;
            }
            if (error2 < dx)
            {
                error += dx;
                pos.Y += step.Y;
            }
        }
    } 
    
    public static List<Coord> BresenhamLine(Coord start, Coord end, bool endPreemptively = false)
    {
        BresenhamLine(Coords, start, end, endPreemptively);
        return Coords;
    }
}