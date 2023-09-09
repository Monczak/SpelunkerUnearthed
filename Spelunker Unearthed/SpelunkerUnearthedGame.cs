using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpelunkerUnearthed.Engine;
using SpelunkerUnearthed.Engine.Rendering;
using SpelunkerUnearthed.Engine.Services;
using SpelunkerUnearthed.Engine.Tiles;

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

        scene = new Scene();

        Entity tilemap = new("Tilemap");
        tilemap.AttachComponent(new Tilemap(10, 10));
        tilemap.AttachComponent(new TileRenderer(GraphicsDevice, tilemap.GetComponent<Tilemap>()));
        scene.AddEntity(tilemap);
        
        base.Initialize();
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);

        ServiceRegistry.Get<TileLoader>().LoadTiles();
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

        // TODO: Render with a camera instead

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        scene.Render(spriteBatch);
        spriteBatch.End();

        base.Draw(gameTime);
    }
}