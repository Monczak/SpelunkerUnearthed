using System;
using MariEngine.Logging;
using MariEngine.Sprites;
using MariEngine.Tiles;
using MariEngine.Utils;
using Microsoft.Xna.Framework.Input;

namespace MariEngine.UI.Nodes.Components;

public class SliderComponent(Sprite background, Sprite inactiveBackground, Sprite bar, int minValue, int maxValue, int step = 1) : ComponentNode, IComponentSelectable<SliderComponent>, IUiCommandReceiver
{
    public Sprite Background => background;
    public Sprite InactiveBackground => inactiveBackground;
    public Sprite Bar => bar;

    public int MinValue => minValue;
    public int MaxValue => maxValue;

    public float FillAmount => MathUtils.InverseLerp(MinValue, MaxValue, Value);
    
    private int value;

    public int Value
    {
        get => value;
        set
        {
            this.value = Math.Min(Math.Max(value, minValue), maxValue);
            ValueChanged?.Invoke(this, Value);
        }
    }

    public event Action<SliderComponent> Selected;
    public event Action<SliderComponent> Deselected;

    public delegate void ValueChangedEventHandler(SliderComponent sender, int newValue);

    public event ValueChangedEventHandler ValueChanged;

    public bool IsSelected { get; set; }

    public void OnSelected()
    {
        Selected?.Invoke(this);
    }

    public void OnDeselected()
    {
        Deselected?.Invoke(this);
    }

    public bool Selectable { get; set; } = true;
    
    public Direction InhibitedNavigationDirections => Direction.Horizontal;
    public bool SelectFirstChild { get; init; } = false;

    public void HandleCommand(UiCommand command)
    {
        switch (command)
        {
            case InputKeyUiCommand { Key: Keys.Right, IsPressed: true }:
                Value += step;
                break;
            case InputKeyUiCommand { Key: Keys.Left, IsPressed: true }:
                Value -= step;
                break;
        }
        Logger.LogDebug(Value);
    }

    public override void Accept(ICanvasRendererVisitor rendererVisitor, TileBufferFragment buffer)
    {
        rendererVisitor.Visit(this, buffer);
    }
}