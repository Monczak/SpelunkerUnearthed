using MariEngine.Audio;
using MariEngine.Components;
using MariEngine.Logging;
using Microsoft.Xna.Framework;

namespace SpelunkerUnearthed.Scripts.TileEntities;

public class AudioTester : TileEntityComponent
{
    private double lastPlayTime;
    
    public override void Update(GameTime gameTime)
    {
        if (gameTime.TotalGameTime.TotalSeconds - lastPlayTime > 0.5f)
        {
            GetComponent<TileEntityAudioSource>().Play("Test");
            lastPlayTime = gameTime.TotalGameTime.TotalSeconds;
        }
    }
}