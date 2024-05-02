using System.Collections.Generic;
using FMOD;
using MariEngine.Logging;
using MariEngine.Services;
using MariEngine.Sprites;
using MariEngine.Tiles;
using MariEngine.UI.Nodes;
using MariEngine.UI.Nodes.Components;
using MariEngine.UI.Nodes.Layouts;

namespace MariEngine.UI;

public class CanvasRendererVisitor : ICanvasRendererVisitor
{
    public void Visit(CanvasNode node, TileBufferFragment buffer)
    {
        Logger.LogDebug("Visit CanvasNode");
    }

    public void Visit(LayoutNode node, TileBufferFragment buffer)
    {
        if (node.Background is not null)
            RenderBackground(node.Background, buffer);
    }

    private static void RenderBackground(Sprite background, TileBufferFragment buffer)
    {
        // TODO: Support non-9-sliced sprites
        foreach (Coord coord in buffer.Bounds.Coords)
        {
            buffer.SetAbsolute(coord, background.GetNineSlice(buffer.Bounds, coord));
        }
    }

    public void Visit(ComponentNode node, TileBufferFragment buffer)
    {
        Logger.LogDebug("Visit ComponentNode");
    }

    // TODO: Add support for text alignment
    public void Visit(TextComponent node, TileBufferFragment buffer)
    {
        RenderText(node, buffer);
    }

    private static void RenderText(TextComponent node, TileBufferFragment buffer)
    {
        var coord = Coord.Zero;
        int i = -1;
        var characters = new Queue<char>(node.Text);

        var lineBreakOpportunities = new Queue<int>();
        for (int j = 1; j < node.Text.Length; j++)
        {
            if (IsLineBreakOpportunity(node.Text[j]) && !IsLineBreakOpportunity(node.Text[j - 1]))
                lineBreakOpportunities.Enqueue(j);
        }
        lineBreakOpportunities.Enqueue(node.Text.Length);

        lineBreakOpportunities.TryDequeue(out var currentLineBreakOpportunity);
        while (characters.TryDequeue(out var c))
        {
            i++;
            lineBreakOpportunities.TryPeek(out var nextLineBreakOpportunity);
            if (i == currentLineBreakOpportunity)
            {
                var wordLength = nextLineBreakOpportunity - currentLineBreakOpportunity;
                lineBreakOpportunities.TryDequeue(out currentLineBreakOpportunity);
                if (coord.X + wordLength > buffer.Bounds.Size.X || c == '\n')
                {
                    LineBreak(ref coord);
                    continue;
                }
            }
            
            if (buffer.IsInBounds(coord))
            {
                buffer[coord] = ServiceRegistry.Get<TileLoader>().GetCharacter(c);
                coord += Coord.UnitX;
            }
        }

        return;

        void LineBreak(ref Coord coord)
        {
            coord.X = 0;
            coord.Y += node.LineSpacing + 1;
        }

        bool IsLineBreakOpportunity(char c) => c is ' ' or '\n';
    }

    public void Visit(ButtonComponent node, TileBufferFragment buffer)
    {
        if (node.Background is not null)
            RenderBackground(node.Background, buffer);
    }
}