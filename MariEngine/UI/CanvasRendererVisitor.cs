using System.Collections.Generic;
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
    public void Render(CanvasNode node, TileBufferFragment buffer)
    {
        Logger.LogDebug("Visit CanvasNode");
    }

    public void Render(LayoutNode node, TileBufferFragment buffer)
    {
        if (node.Background is not null)
            RenderBackground(node.Background, buffer);
    }

    private static void RenderBackground(Sprite background, TileBufferFragment buffer)
    {
        // TODO: Support non-9-sliced sprites
        foreach (Coord coord in buffer.Bounds.Coords)
        { 
            if (buffer.IsInOuterBounds(coord))
                buffer.SetAbsolute(coord, background.GetNineSlice(buffer.Bounds, coord));
        }
    }

    public void Render(ComponentNode node, TileBufferFragment buffer)
    {
        Logger.LogDebug("Visit ComponentNode");
    }

    
    public void Render(TextComponent node, TileBufferFragment buffer)
    {
        RenderText(node, buffer);
    }

    private static Coord MeasureText(string text, int lineSpacing, int xSize) =>
        RenderText(text, lineSpacing, null, xSize);

    private static Coord RenderText(string text, int lineSpacing, TileBufferFragment buffer) =>
        RenderText(text, lineSpacing, buffer, buffer.Bounds.Size.X);
    
    private static Coord RenderText(TextComponent node, TileBufferFragment buffer) =>
        RenderText(node.Text, node.LineSpacing, buffer, buffer.Bounds.Size.X);

    // TODO: Add support for text alignment
    private static Coord RenderText(string text, int lineSpacing, TileBufferFragment buffer, int xSize)
    {
        var textSize = Coord.Zero;
        
        var coord = Coord.Zero;
        int i = -1;
        var characters = new Queue<char>(text);

        var lineBreakOpportunities = new Queue<int>();
        for (int j = 1; j < text.Length; j++)
        {
            if (IsLineBreakOpportunity(text[j]) && !IsLineBreakOpportunity(text[j - 1]))
                lineBreakOpportunities.Enqueue(j);
        }
        lineBreakOpportunities.Enqueue(text.Length);

        lineBreakOpportunities.TryDequeue(out var currentLineBreakOpportunity);
        while (characters.TryDequeue(out var c))
        {
            i++;
            lineBreakOpportunities.TryPeek(out var nextLineBreakOpportunity);
            if (i == currentLineBreakOpportunity)
            {
                var wordLength = nextLineBreakOpportunity - currentLineBreakOpportunity;
                lineBreakOpportunities.TryDequeue(out currentLineBreakOpportunity);
                if (coord.X + wordLength > xSize || c == '\n')
                {
                    LineBreak(ref coord);
                    continue;
                }
            }
            
            if (buffer is not null && buffer.IsInBounds(coord) && buffer.IsInOuterBounds(coord + buffer.Bounds.TopLeft))
            {
                buffer[coord] = ServiceRegistry.Get<TileLoader>().GetCharacter(c);
            }
            
            textSize = Coord.Max(textSize, coord + Coord.One);
            coord += Coord.UnitX;
        }

        return textSize;

        void LineBreak(ref Coord coord)
        {
            coord.X = 0;
            coord.Y += lineSpacing + 1;
        }

        bool IsLineBreakOpportunity(char c) => c is ' ' or '\n';
    }

    public void Render(ButtonComponent node, TileBufferFragment buffer)
    {
        if (node.IsSelected)
        {
            if (node.Background is not null) RenderBackground(node.Background, buffer);
        }
        else
        {
            if (node.InactiveBackground is not null) RenderBackground(node.InactiveBackground, buffer);
        }
        

        var labelSize = buffer.Bounds.Size - Coord.One * node.TextPadding * 2;
        
        var textSize = MeasureText(node.Label, 1, labelSize.X);
        var textBufferSize = Coord.Min(textSize, labelSize);
        var textBufferTopLeft = Coord.One * node.TextPadding + (labelSize - textBufferSize) / 2;
        var textBuffer = new TileBufferFragment(buffer, new CoordBounds(textBufferTopLeft, textBufferSize));
        RenderText(node.Label, 1, textBuffer);
    }

    public void Render(SliderComponent node, TileBufferFragment buffer)
    {
        if (node.IsSelected)
        {
            if (node.Background is not null) RenderBackground(node.Background, buffer);
        }
        else
        {
            if (node.InactiveBackground is not null) RenderBackground(node.InactiveBackground, buffer);
        }

        var fillPercentage = node.FillAmount;
        var barBounds = new CoordBounds(buffer.Bounds.TopLeft,
            new Coord(buffer.Bounds.Size.X * fillPercentage, buffer.Bounds.Size.Y));
        foreach (var coord in barBounds.Coords)
        {
            if (buffer.IsInOuterBounds(coord))
                buffer.SetAbsolute(coord, node.Bar.GetNineSlice(barBounds, coord));
        }
    }
}