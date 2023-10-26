using System.IO;

namespace MariEngine;

public static class ContentPaths
{
    public static string Content => "Content";
    
    public static string Tiles => Path.Combine(Content, "Tiles");
    public static string Fonts => Path.Combine(Content, "Fonts");

    public static string WorldGen => Path.Combine(Content, "World Gen");
    public static string Biomes => Path.Combine(WorldGen, "Biomes");
}