using System;
using Microsoft.Xna.Framework;

namespace MariEngine.Utils;

public static class ColorUtils
{
    public static Color FromHex(string hexColor)
    {
        if (hexColor[0] == '#') hexColor = hexColor[1..];

        byte[] rgb = Convert.FromHexString(hexColor);
        return new Color(rgb[0], rgb[1], rgb[2]);
    }
}