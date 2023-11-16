namespace MariEngine.Utils;

public static class RandomUtils
{
    public static int Hash(int input)
    {
        unchecked
        {
            int state = (int)(input * 747796405 + 2891336453);
            int word = ((state >> ((state >> 28) + 4)) ^ state) * 277803737;
            return (word >> 22) ^ word;
        }
    }
}