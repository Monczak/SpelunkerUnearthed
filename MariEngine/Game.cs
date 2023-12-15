using FmodForFoxes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using INITFLAGS = FMOD.Studio.INITFLAGS;

namespace MariEngine;

public abstract class Game : Microsoft.Xna.Framework.Game
{
    private readonly INativeFmodLibrary nativeFmodLibrary;
    protected readonly GraphicsDeviceManager Graphics;
    protected SpriteBatch SpriteBatch;

    protected Game(INativeFmodLibrary nativeFmodLibrary)
    {
        this.nativeFmodLibrary = nativeFmodLibrary;
        Graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = ContentPaths.Content;
    }
    
    protected void InitializeAudio(string contentPath)
    {
        FmodManager.Init(nativeFmodLibrary, FmodInitMode.CoreAndStudio, contentPath, studioInitFlags: INITFLAGS.LIVEUPDATE);
    }

    protected void UnloadAudio()
    {
        FmodManager.Unload();
    }
}