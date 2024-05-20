using System;

namespace MariEngine.Services;

public class SceneManager : Service
{
    internal event EventHandler<Scene> SceneLoaded;
    internal event EventHandler<Type> SceneTypeLoaded; 

    public void LoadScene(Scene scene)
    {
        SceneLoaded?.Invoke(this, scene);
    }

    public void LoadScene<T>()
    {
        SceneTypeLoaded?.Invoke(this, typeof(T));
    }
}