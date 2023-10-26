using System.IO;

namespace MariEngine;

public class ContentPaths
{
    public static string Content => "Content";
    
    public static string Tiles => Path.Combine(Content, "Tiles");
    public static string Fonts => Path.Combine(Content, "Fonts");
}