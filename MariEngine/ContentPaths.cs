using System.IO;

namespace MariEngine;

public class ContentPaths
{
    public static string Content => "Content";
    
    public static string Tiles => Path.Combine(Content, "Tiles");
    public static string Fonts => Path.Combine(Content, "Fonts");
    public static string Sprites => Path.Combine(Content, "Sprites");
    public static string Materials => Path.Combine(Content, "Materials");
}