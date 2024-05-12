using System;
using MariEngine.Utils;
using Microsoft.Xna.Framework;

namespace MariEngine.Animation;

public class Tween
{
    public delegate float TransitionFunction(float time);

    private TransitionFunction transitionFunction;
    
    private Action<float> updateAction;
    private readonly float transitionTime;
    private double startTime;

    private bool started;

    public event Action<Tween> Finished;

    internal bool ToBeReleased { get; private set; } = false;
    private readonly bool releaseOnFinished;

    public Tween(Action<float> updateAction, TransitionFunction transitionFunction, float transitionTime, bool releaseOnFinished = true)
    {
        this.updateAction = updateAction;
        this.transitionFunction = transitionFunction;
        this.transitionTime = transitionTime;
        this.releaseOnFinished = releaseOnFinished;
        
        if (transitionTime <= 0)
            throw new ArgumentException("Tween transition time must be non-negative.");
    }

    internal void Update(GameTime gameTime)
    {
        if (!started)
        {
            startTime = gameTime.TotalGameTime.TotalSeconds;
            started = true;
        }

        var normalizedTime = MathUtils.Clamp((float)((gameTime.TotalGameTime.TotalSeconds - startTime) / transitionTime), 0, 1);
        updateAction?.Invoke(transitionFunction(normalizedTime));
        
        if (normalizedTime >= 1f)
        {
            Finished?.Invoke(this);
            if (releaseOnFinished) Release();
        }
    }

    public void Release()
    {
        ToBeReleased = true;
    }
}