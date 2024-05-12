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

    private float initialValue;
    private float targetValue;

    private bool started;

    public event Action<Tween> Finished;

    internal bool ToBeReleased { get; private set; } = false;
    private readonly bool releaseOnFinished;

    public Tween(Action<float> updateAction, TransitionFunction transitionFunction, float transitionTime, float initialValue, float targetValue, bool releaseOnFinished = true)
    {
        this.updateAction = updateAction;
        this.transitionFunction = transitionFunction;
        this.transitionTime = transitionTime;
        this.releaseOnFinished = releaseOnFinished;
        
        this.initialValue = initialValue;
        this.targetValue = targetValue;
        
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
        var transition = transitionFunction(normalizedTime);
        var tweenedValue = MathUtils.LerpUnclamped(initialValue, targetValue, transition);
        updateAction?.Invoke(tweenedValue);
        
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