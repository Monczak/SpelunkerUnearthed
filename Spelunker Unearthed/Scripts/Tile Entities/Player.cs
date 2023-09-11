using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SpelunkerUnearthed.Engine.Tiles;

namespace SpelunkerUnearthed.Scripts.TileEntities;

public class Player : TileEntity
{
    public override void Update(GameTime gameTime)
    {
        var state = Keyboard.GetState();
        if (state.IsKeyDown(Keys.Right))
            Move(dx: 1);
        if (state.IsKeyDown(Keys.Left))
            Move(dx: -1);
        if (state.IsKeyDown(Keys.Up))
            Move(dy: -1);
        if (state.IsKeyDown(Keys.Down))
            Move(dy: 1);
    }
}