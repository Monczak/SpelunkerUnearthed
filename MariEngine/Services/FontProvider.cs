using System.Collections.Generic;
using System.IO;
using FontStashSharp;

namespace MariEngine.Services;

public class FontProvider : Service
{
    private readonly Dictionary<string, FontSystem> fontSystems = new();

    public FontSystem GetFontSystem(string id) => fontSystems[id];
    
    public void AddFont(string fontSystemId, string fontName)
    {
        if (!fontSystems.ContainsKey(fontSystemId))
            fontSystems[fontSystemId] = new FontSystem();
        
        fontSystems[fontSystemId].AddFont(File.ReadAllBytes(Path.Combine(ContentPaths.Fonts, $"{fontName}.ttf")));
    }

    public SpriteFontBase GetFont(string fontSystemId, float size) => GetFontSystem(fontSystemId).GetFont(size);
}