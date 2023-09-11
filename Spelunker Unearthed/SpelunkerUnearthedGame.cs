using System.Linq;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpelunkerUnearthed.Engine;
using SpelunkerUnearthed.Engine.Components;
using SpelunkerUnearthed.Engine.Logging;
using SpelunkerUnearthed.Engine.Rendering;
using SpelunkerUnearthed.Engine.Services;
using SpelunkerUnearthed.Engine.Tiles;
using SpelunkerUnearthed.Scripts.TileEntities;

namespace SpelunkerUnearthed;

public class SpelunkerUnearthedGame : Game
{
    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;

    private Scene scene;

    public SpelunkerUnearthedGame()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        Window.AllowUserResizing = true;
    }

    protected override void Initialize()
    {
        FontSystemDefaults.FontResolutionFactor = 4.0f;
        FontSystemDefaults.KernelWidth = 4;
        FontSystemDefaults.KernelHeight = 4;
        
        ServiceRegistry.RegisterService(new TileLoader());

        scene = new Scene(Window);

        Entity tilemap = new("Tilemap");
        tilemap.AttachComponent(new Transform());
        tilemap.AttachComponent(new Tilemap(10, 10));
        tilemap.AttachComponent(new TilemapRenderer(GraphicsDevice, tilemap.GetComponent<Tilemap>()));
        scene.AddEntity(tilemap);
        
        base.Initialize();
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);

        ServiceRegistry.Get<TileLoader>().LoadTiles();
        
        // TODO: Remove this, this is for testing only
        var tilemap = scene.Entities.First(e => e.Name == "Tilemap").GetComponent<Tilemap>();
        var player = new Player { Tile = ServiceRegistry.Get<TileLoader>().GetTile("Player") };
        tilemap.AddTileEntity(player);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        scene.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        
        scene.Render(spriteBatch);
        
        base.Draw(gameTime);
    }
}