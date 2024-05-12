using System;

namespace MariEngine.Animation;

public class TweenBuilder
{
    private TweenTransition transition = TweenTransition.Quadratic;
    private TweenEasing easing = TweenEasing.EaseOut;
    
    private Tween.TransitionFunction customTransitionFunction;
    
    private float transitionTime = 1.0f;
    private Action<float> updateAction;
    private bool releaseOnFinished = true;

    public TweenBuilder WithTransition(TweenTransition transition)
    {
        this.transition = transition;
        return this;
    }

    public TweenBuilder WithEasing(TweenEasing easing)
    {
        this.easing = easing;
        return this;
    }

    public TweenBuilder WithCustomTransition(Tween.TransitionFunction transitionFunction)
    {
        customTransitionFunction = transitionFunction;
        return this;
    }
    
    public TweenBuilder WithTime(float time)
    {
        transitionTime = time;
        return this;
    }
    
    public TweenBuilder WithProperty(Action<float> updateAction)
    {
        this.updateAction = updateAction;
        return this;
    }

    public TweenBuilder Persistent()
    {
        releaseOnFinished = false;
        return this;
    }

    internal Tween Build()
    {
        var transitionFunction = customTransitionFunction ?? TweenFunctions.Lookup(transition, easing);
        return new Tween(updateAction, transitionFunction, transitionTime, releaseOnFinished);
    }
}