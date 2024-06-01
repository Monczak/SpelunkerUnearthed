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
    public bool Enabled { get; set; }

    private Dictionary<object, List<DebugScreenLine>> Lines { get; } = [];
    
    public void AddLine(object context, DebugScreenLine line)
    {
        Lines.TryAdd(context, []);
        Lines[context].Add(line);
    }

    public void RemoveAllLines(object context)
    {
        Lines.Remove(context);
    }

    // TODO: Refactor this out to a DebugScreenRenderer?
    public void Render(SpriteBatch spriteBatch)
    {
        var font = ServiceRegistry.Get<FontProvider>().GetFont("Monospace", 12);
        
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp);

        StringBuilder builder = new();
        foreach (var (context, contextLines) in Lines)
        {
            foreach (var line in contextLines)
                builder.AppendLine(line.GetLine());
        }
        spriteBatch.DrawString(font, builder, Vector2.Zero, Color.White);

        spriteBatch.End();
    }
}