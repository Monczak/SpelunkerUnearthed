using System;
using Microsoft.Xna.Framework;

namespace MariEngine.Utils;

public static class ColorUtils
{
    public static Color FromHex(string hexColor)
    {
        if (hexColor[0] == '#') hexColor = hexColor[1..];

        byte[] rgba = Convert.FromHexString(hexColor);
        return new Color(rgba[0], rgba[1], rgba[2], rgba.Length == 4 ? rgba[3] : 255);
    }
}