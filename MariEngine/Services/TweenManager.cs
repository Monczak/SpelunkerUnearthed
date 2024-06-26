﻿using System;
using System.Collections.Generic;
using MariEngine.Animation;
using Microsoft.Xna.Framework;

namespace MariEngine.Services;

public class TweenManager : Service
{
    private readonly List<Tween> activeTweens = [];

    public Tween BuildTween(Func<TweenBuilder, TweenBuilder> builder)
    {
        var theBuilder = new TweenBuilder();
        var tween = builder(theBuilder).Build();
        activeTweens.Add(tween);
        return tween;
    }
    
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        
        foreach (var tween in activeTweens)
            tween.Update(gameTime);

        activeTweens.RemoveAll(tween => tween.ToBeReleased);
    }
}