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
    }

    private void InitializeBuffer()
    {
        Coord screenSize = new(graphicsDevice.PresentationParameters.BackBufferWidth, graphicsDevice.PresentationParameters.BackBufferHeight);
        screenSize /= ServiceRegistry.Get<TileAtlas>().TileSize;
        tileBuffer = new TileBuffer(screenSize + Coord.One * overscan * 2);
        
        foreach (Coord coord in tileBuffer.Coords)
        {
            tileBuffer[coord] = ServiceRegistry.Get<TileLoader>().Get("Player");
        }
    }

    protected override void OnAttach()
    {
        canvas = GetComponent<Canvas>();
        testTexture = ServiceRegistry.Get<TexturePool>().RequestTexture(Coord.One, out _);
        testTexture.SetData(new[] { Color.Aqua });
    }

    public void Redraw()
    {
        var layout = LayoutEngine.CalculateLayout(canvas.Root);
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