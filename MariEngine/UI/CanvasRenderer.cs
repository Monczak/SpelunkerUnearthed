using System;
using System.Collections.Generic;
using MariEngine.Events;
using MariEngine.Logging;
using MariEngine.Rendering;
using MariEngine.Services;
using MariEngine.Tiles;
using MariEngine.UI.Nodes;
using MariEngine.UI.Nodes.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MariEngine.UI;

public partial class CanvasRenderer : Renderer
{
    private Canvas canvas;
    private TileBuffer tileBuffer;
    private Texture2D testTexture;

    private int overscan;

    private ICanvasRendererVisitor rendererVisitor;

    private Dictionary<CanvasNode, CoordBounds> layout;

    private bool redrawEveryFrame;
    
    public CanvasRenderer(GraphicsDevice graphicsDevice, Camera camera, int overscan = 1, ICanvasRendererVisitor rendererVisitor = null, bool redrawEveryFrame = false) : base(graphicsDevice, camera)
    {
        this.overscan = overscan;
        this.rendererVisitor = rendererVisitor ?? new CanvasRendererVisitor();
        this.redrawEveryFrame = redrawEveryFrame;
        
        InitializeBuffer();
        
        ServiceRegistry.Get<EventManager>().Bind(this, "ClientSizeChanged", Reinitialize);
        ServiceRegistry.Get<EventManager>().Bind(this, "TileAtlasResized", new Action<int>(_ => Reinitialize()));
    }

    private void Reinitialize()
    {
        InitializeBuffer();
        RecomputeLayout();
        Redraw();
    }

    private void InitializeBuffer()
    {
        Coord screenSize = new(GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);
        screenSize /= ServiceRegistry.Get<TileAtlas>().TileSize;
        tileBuffer = new TileBuffer(screenSize + Coord.One * overscan * 2);
    }

    protected override void OnAttach()
    {
        canvas = GetComponent<Canvas>();
        testTexture = ServiceRegistry.Get<TexturePool>().RequestTexture(Coord.One, out _);
        testTexture.SetData([Color.Aqua]);
    }

    public void RecomputeLayout()
    {
        canvas.Root.Padding = Coord.One * overscan;
        layout = LayoutEngine.CalculateLayout(canvas.Root, new Coord(tileBuffer.Width, tileBuffer.Height));
    }
    
    public void Redraw(bool recomputeLayout = false)
    {
        if (recomputeLayout) RecomputeLayout();
        
        // TODO: Add dirtying system
        foreach (var (node, bounds) in layout)
        {
            // string tileId = LayoutEngine.DepthMap[node] switch
            // {
            //     0 => "Nothing",
            //     1 => "Stone",
            //     2 => "Player",
            //     3 => "Scoria",
            //     _ => "PackedIce",
            // };
            // foreach (Coord coord in bounds.Coords)
            // {
            //     if (tileBuffer.IsInBounds(coord))
            //         tileBuffer[coord] = ServiceRegistry.Get<TileLoader>().Get(tileId);
            // }
            
            node.Accept(rendererVisitor, tileBuffer.GetFragment(bounds));
        }
    }

    protected override void Render(SpriteBatch spriteBatch)
    {
        if (redrawEveryFrame) Redraw();
        
        spriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, SamplerState.AnisotropicClamp);

        foreach (var coord in tileBuffer.Coords)
        {
            var tile = tileBuffer[coord];
            if (tile is not null)
            {
                ServiceRegistry.Get<TileAtlas>().DrawTile(spriteBatch, ((Vector2)coord - Vector2.One * overscan) * Camera.TileSize + CalculateCenterOffset(), tile.Id, Color.White);
            }
        }
            
        spriteBatch.End();
    }

    protected override Vector2 CalculateCenterOffset()
    {
        Vector2 screenSize = new(GraphicsDevice.PresentationParameters.BackBufferWidth,
            GraphicsDevice.PresentationParameters.BackBufferHeight);
        Vector2 uiSize = new Vector2(tileBuffer.Width, tileBuffer.Height) - Vector2.One * overscan * 2;
        Vector2 leftover = screenSize - uiSize * Camera.TileSize;
        return leftover / 2;
    }

    protected override void OnDestroy()
    {
        ServiceRegistry.Get<EventManager>().UnbindAll(this);
    }
}