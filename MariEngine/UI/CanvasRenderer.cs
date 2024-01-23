using System;
using System.Collections.Generic;
using MariEngine.Events;
using MariEngine.Rendering;
using MariEngine.Services;
using MariEngine.Tiles;
using MariEngine.UI.Nodes;
using MariEngine.UI.Nodes.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MariEngine.UI;

public class CanvasRenderer : Renderer
{
    private Canvas canvas;
    private TileBuffer tileBuffer;
    private Texture2D testTexture;

    private int overscan;

    public CanvasRenderer(GraphicsDevice graphicsDevice, Camera camera, int overscan = 1) : base(graphicsDevice, camera)
    {
        this.overscan = overscan;
        
        InitializeBuffer();
        
        ServiceRegistry.Get<EventManager>().Bind(this, "ClientSizeChanged", OnClientSizeChanged);
    }

    private void OnClientSizeChanged()
    {
        InitializeBuffer();
        Redraw();
    }

    private void InitializeBuffer()
    {
        Coord screenSize = new(graphicsDevice.PresentationParameters.BackBufferWidth, graphicsDevice.PresentationParameters.BackBufferHeight);
        screenSize /= ServiceRegistry.Get<TileAtlas>().TileSize;
        tileBuffer = new TileBuffer(screenSize + Coord.One * overscan * 2);
    }

    protected override void OnAttach()
    {
        canvas = GetComponent<Canvas>();
        testTexture = ServiceRegistry.Get<TexturePool>().RequestTexture(Coord.One, out _);
        testTexture.SetData([Color.Aqua]);
    }

    public void Redraw()
    {
        canvas.Root.Padding = Coord.One * overscan;
        
        var layout = LayoutEngine.CalculateLayout(canvas.Root, new Coord(tileBuffer.Width, tileBuffer.Height));
        foreach (var (node, bounds) in layout)
        {
            string tileId = LayoutEngine.DepthMap[node] switch
            {
                0 => "Nothing",
                1 => "Stone",
                2 => "Player",
                3 => "Scoria",
                _ => "PackedIce",
            };
            foreach (Coord coord in bounds.Coords)
            {
                if (tileBuffer.IsInBounds(coord))
                    tileBuffer[coord] = ServiceRegistry.Get<TileLoader>().Get(tileId);
            }
        }
    }

    protected override void Render(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, SamplerState.PointClamp);

        foreach (var coord in tileBuffer.Coords)
        {
            var tile = tileBuffer[coord];
            if (tile is not null)
            {
                ServiceRegistry.Get<TileAtlas>().DrawTile(spriteBatch, ((Vector2)coord - Vector2.One * overscan) * camera.TileSize + CalculateCenterOffset(), tile.Id, Color.White);
            }
        }
            
        spriteBatch.End();
    }

    protected override Vector2 CalculateCenterOffset()
    {
        Vector2 screenSize = new(graphicsDevice.PresentationParameters.BackBufferWidth,
            graphicsDevice.PresentationParameters.BackBufferHeight);
        Vector2 uiSize = new Vector2(tileBuffer.Width, tileBuffer.Height) - Vector2.One * overscan * 2;
        Vector2 leftover = screenSize - uiSize * camera.TileSize;
        return leftover / 2;
    }

    protected override void OnDestroy()
    {
        ServiceRegistry.Get<EventManager>().UnbindAll(this);
    }
}