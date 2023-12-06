using System.IO;

namespace SpelunkerUnearthed.Scripts;

public class ContentPaths : MariEngine.ContentPaths
{
    public static string WorldGen => Path.Combine(Content, "World Gen");
    public static string Biomes => Path.Combine(WorldGen, "Biomes");
    public static string Features => Path.Combine(WorldGen, "Features");
}