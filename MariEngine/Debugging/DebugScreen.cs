using System.Collections.Generic;
using System.IO;
using System.Text;
using FontStashSharp;
using MariEngine.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MariEngine.Debugging;

public class DebugScreen : Service
{
    private FontSystem fontSystem;
    
    public bool Enabled { get; set; }
    
    internal List<DebugScreenLine> Lines { get; private set; } = new();

    public DebugScreen()
    {
        fontSystem = new FontSystem();
        
        // TODO: This is repeated code from TileAtlas - refactor it out to some text utility
        // (also don't hardcode paths)
        fontSystem.AddFont(File.ReadAllBytes($"Content/Fonts/Hack-Regular.ttf"));
    }

    public void AddLine(DebugScreenLine line)
    {
        Lines.Add(line);
    }

    // TODO: Refactor this out to a DebugScreenRenderer?
    public void Render(SpriteBatch spriteBatch)
    {
        var font = fontSystem.GetFont(12);
        
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp);

        StringBuilder builder = new();
        foreach (var line in Lines)
        {
            builder.AppendLine(line.GetLine());
        }
        spriteBatch.DrawString(font, builder, Vector2.Zero, Color.White);

        spriteBatch.End();
    }
}