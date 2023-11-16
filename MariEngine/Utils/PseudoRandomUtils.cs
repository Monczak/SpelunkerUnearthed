using System;

namespace MariEngine.Utils;

public static class PseudoRandomUtils
{
    public static int Hash(int x)
    {
        unchecked
        {
            x = ((x >> 16) ^ x) * 0x45d9f3b + 1;
            x = ((x >> 16) ^ x) * 0x45d9f3b + 1;
            x = (x >> 16) ^ x + 1;
            return x;
        }
    }
}