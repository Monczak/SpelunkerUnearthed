using System;

namespace MariEngine.Animation;

internal static class TweenFunctions
{
    public static Tween.TransitionFunction Lookup(TweenTransition transition, TweenEasing easing = TweenEasing.EaseOut) =>
        (transition, easing) switch
        {
            (TweenTransition.Linear, _) => Linear,

            (TweenTransition.Quadratic, TweenEasing.EaseIn) => EaseInQuadratic,
            (TweenTransition.Quadratic, TweenEasing.EaseOut) => EaseOutQuadratic,
            (TweenTransition.Quadratic, TweenEasing.EaseInOut) => EaseInOutQuadratic,

            (TweenTransition.Cubic, TweenEasing.EaseIn) => EaseInCubic,
            (TweenTransition.Cubic, TweenEasing.EaseOut) => EaseOutCubic,
            (TweenTransition.Cubic, TweenEasing.EaseInOut) => EaseInOutCubic,

            (TweenTransition.Sine, TweenEasing.EaseIn) => EaseInSine,
            (TweenTransition.Sine, TweenEasing.EaseOut) => EaseOutSine,
            (TweenTransition.Sine, TweenEasing.EaseInOut) => EaseInOutSine,

            (TweenTransition.Exponential, TweenEasing.EaseIn) => EaseInExpo,
            (TweenTransition.Exponential, TweenEasing.EaseOut) => EaseOutExpo,
            (TweenTransition.Exponential, TweenEasing.EaseInOut) => EaseInOutExpo,

            (TweenTransition.Elastic, TweenEasing.EaseIn) => EaseInElastic,
            (TweenTransition.Elastic, TweenEasing.EaseOut) => EaseOutElastic,
            (TweenTransition.Elastic, TweenEasing.EaseInOut) => EaseInOutElastic,

            (TweenTransition.Bouncy, TweenEasing.EaseIn) => EaseInBouncy,
            (TweenTransition.Bouncy, TweenEasing.EaseOut) => EaseOutBouncy,
            (TweenTransition.Bouncy, TweenEasing.EaseInOut) => EaseInOutBouncy,

            _ => throw new ArgumentException("Unknown transition-easing combination.")
        };

    private static float Linear(float t) => t;

    private static float EaseInQuadratic(float t) => t * t;
    private static float EaseOutQuadratic(float t) => 1 - (1 - t) * (1 - t);
    private static float EaseInOutQuadratic(float t) => t < 0.5f ? 2 * t * t : 1 - MathF.Pow(-2 * t + 2, 2) / 2;

    private static float EaseInCubic(float t) => t * t * t;
    private static float EaseOutCubic(float t) => 1 - (1 - t) * (1 - t) * (1 - t);
    private static float EaseInOutCubic(float t) => t < 0.5 ? 4 * t * t * t : 1 - MathF.Pow(-2 * t + 2, 3) / 2;

    private static float EaseInSine(float t) => 1 - MathF.Cos((t * MathF.PI) / 2);
    private static float EaseOutSine(float t) => MathF.Sin((t * MathF.PI) / 2);
    private static float EaseInOutSine(float t) => -(MathF.Cos(t * MathF.PI) - 1) / 2;

    private static float EaseInExpo(float t) => MathF.Abs(t) < float.Epsilon ? 0 : MathF.Pow(2, 10 * t - 10);
    private static float EaseOutExpo(float t) => MathF.Abs(t - 1) < float.Epsilon ? 1 : 1 - MathF.Pow(2, -10 * t);

    private static float EaseInOutExpo(float t) => MathF.Abs(t) < float.Epsilon
        ? 0
        : MathF.Abs(t - 1) < float.Epsilon
            ? 1
            : t < 0.5f
                ? MathF.Pow(2, 20 * t - 10) / 2
                : (2 - MathF.Pow(2, -20 * t + 10)) / 2;

    private static float EaseInElastic(float t)
    {
        const float c4 = (2 * MathF.PI / 3);
        return MathF.Abs(t) < float.Epsilon
            ? 0
            : MathF.Abs(t - 1) < float.Epsilon
                ? 1
                : -MathF.Pow(2, 10 * t - 10) * MathF.Sin((t * 10 - 10.75f) * c4);
    }

    private static float EaseOutElastic(float t)
    {
        const float c4 = (2 * MathF.PI / 3);
        return MathF.Abs(t) < float.Epsilon
            ? 0
            : MathF.Abs(t - 1) < float.Epsilon
                ? 1
                : MathF.Pow(2, -10 * t) * MathF.Sin((t * 10 - 0.75f) * c4) + 1;
    }

    private static float EaseInOutElastic(float t)
    {
        const float c5 = (2 * MathF.PI) / 4.5f;
        return MathF.Abs(t) < float.Epsilon
            ? 0
            : MathF.Abs(t - 1) < float.Epsilon
                ? 1
                : t < 0.5f
                    ? -(MathF.Pow(2, 20 * t - 10) * MathF.Sin((20 * t - 11.125f) * c5)) / 2
                    : (MathF.Pow(2, -20 * t + 10) * MathF.Sin((20 * t - 11.125f) * c5)) / 2 + 1;
    }

    private static float EaseInBouncy(float t) => 1 - EaseOutBouncy(1 - t);

    private static float EaseOutBouncy(float t)
    {
        const float n1 = 7.5625f;
        const float d1 = 2.75f;

        return t switch
        {
            < 1 / d1 => n1 * t * t,
            < 2 / d1 => n1 * (t -= 1.5f / d1) * t + 0.75f,
            < 2.5f / d1 => n1 * (t -= 2.25f / d1) * t + 0.9375f,
            _ => n1 * (t -= 2.625f / d1) * t + 0.984375f
        };
    }

    private static float EaseInOutBouncy(float t) => t < 0.5f
        ? (1 - EaseOutBouncy(1 - 2 * t)) / 2
        : (1 + EaseOutBouncy(2 * t - 1)) / 2;
}