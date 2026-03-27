using Godot;
using System;
using System.Collections.Generic;
/// <summary>
/// A reusable component for animating UI elements using Tweens, added as a child to any Control node.
/// </summary>
public partial class UiAnimationComponent : Node
{
    private Control ParentControl { get; set; }
    private Dictionary<TweenPropertyEnum, Func<(string propertyName, Variant value)>> _propertyMap;
    private readonly Dictionary<TweenPropertyEnum, Variant> _originalValues = new();
    
    [Export] public TriggerEnum Trigger;
    [Export] public TweenPropertyEnum PropertyTween = TweenPropertyEnum.size;
    [Export] public Tween.EaseType TweenEaseType= Tween.EaseType.In;
    [Export] private float _tweenDuration= 0f;
    [Export] private bool _reverseAnim = false;
    [Export] private float _reverseTweenDuration = 0f;
    [Export] private bool _setPivotCenter = true;
    [ExportGroup("Properties")] 
    [Export] private Vector2 _sizeProp;
    [Export] private Vector2 _scaleProp;
    [Export] private Color _modulateColorProp;
    [Export] private Color _selfModulateColorProp;
    [Export] private Vector2 _positionProp;
    [Export] private Vector2 _globalPositionProp;
    [Export] private Vector2 _rotationProp;
    [Export] private int _fontSizeProp;
    [Export] private Color _fontColorProp;
    [Export] private Vector2 _minimumSizeProp;
    [Export] private Vector2 _marginPaddingProp;
    [Export] private float _progressBarValueProp;
    [Export] private float _labelVisibleRatioProp;
    
    public enum TriggerEnum
    {
        FocusEntered,
        FocusExited,
        VisibilityChanged,
        Ready,
        TreeEntered,
        Pressed,
        Toggled,
        MouseEntered,
        MouseExited,
        TextSubmitted
    }

    public enum TweenPropertyEnum
    {
        size,
        scale,
        modulate,
        self_modulate,
        position,
        global_position,
        rotation,
        font_size,
        font_color,
        custom_minimum_size,
        margin,
        value,
        visible_ratio
    }
    
    public override void _Ready()
    {
        ParentControl = GetParent<Control>();
        if(_setPivotCenter)
            CenterPivot();
        ConnectTrigger();
        MapPropertyDict();
    }

    private void MapPropertyDict()
    {
        _propertyMap = new Dictionary<TweenPropertyEnum, Func<(string, Variant)>> {
            { TweenPropertyEnum.size, () => ("size", _sizeProp) },
            { TweenPropertyEnum.scale, () => ("scale", _scaleProp) },
            { TweenPropertyEnum.modulate, () => ("modulate", _modulateColorProp) },
            { TweenPropertyEnum.self_modulate, () => ("self_modulate", _selfModulateColorProp) },
            { TweenPropertyEnum.position, () => ("position", _positionProp) },
            { TweenPropertyEnum.global_position, () => ("global_position", _globalPositionProp) },
            { TweenPropertyEnum.rotation, () => ("rotation", _rotationProp) },
            { TweenPropertyEnum.font_size, () => ("theme_override_font_sizes/font_size", _fontSizeProp) },
            { TweenPropertyEnum.font_color, () => ("theme_override_colors/font_color", _fontColorProp) },
            { TweenPropertyEnum.custom_minimum_size, () => ("custom_minimum_size", _minimumSizeProp) },
            { TweenPropertyEnum.margin, () => ("margin", _marginPaddingProp) },
            { TweenPropertyEnum.value, () => ("value", _progressBarValueProp) },
            { TweenPropertyEnum.visible_ratio, () => ("visible_ratio", _labelVisibleRatioProp) },
        };
        var (propertyName, value) = _propertyMap[PropertyTween]();
        _originalValues[PropertyTween] = ParentControl.Get(propertyName);
    }

    private void ConnectTrigger()
    {
        switch (Trigger) //great wall of cases
        {
            case TriggerEnum.FocusEntered:
                ParentControl.FocusEntered += PlayAnimation;
                if(_reverseAnim)
                    ParentControl.FocusExited += PlayReverseAnimation;
                break;
            case TriggerEnum.FocusExited:
                ParentControl.FocusExited += PlayAnimation;
                    if(_reverseAnim)
                        ParentControl.FocusEntered += PlayReverseAnimation;
                break;
            case TriggerEnum.VisibilityChanged:
                ParentControl.VisibilityChanged += PlayAnimation;
                break;
            case TriggerEnum.Ready:
                ParentControl.Ready += PlayAnimation;
                break;
            case TriggerEnum.TreeEntered:
                ParentControl.TreeEntered += PlayAnimation;
                break;
            case TriggerEnum.Pressed:
                var pressedParent = GetParent<Button>();
                pressedParent.Pressed += PlayAnimation;
                break;
            case TriggerEnum.Toggled:
                var toggledParent = GetParent<Button>();
                toggledParent.Toggled += PlayAnimation;
                break;
            case TriggerEnum.MouseEntered:
                ParentControl.MouseEntered += PlayAnimation;
                if(_reverseAnim)
                    ParentControl.MouseExited += PlayReverseAnimation;
                break;
            case TriggerEnum.MouseExited:
                ParentControl.MouseExited += PlayAnimation;
                if(_reverseAnim)
                    ParentControl.MouseEntered += PlayReverseAnimation;
                break;
            case TriggerEnum.TextSubmitted:
                var textSubmittedParent = GetParent<LineEdit>();
                textSubmittedParent.TextSubmitted += PlayAnimation;
                break;
            
        }
    }

    private void PlayAnimation(string _)
    {
        PlayAnimation();
    }

    private void PlayAnimation(bool toggle) //maybe if bool == true PlayAnim, if false PlayReverseAnim?
    {
        if (_reverseAnim)
        {
            if (toggle)
            {
                PlayAnimation();
            }
            else
            {
                PlayReverseAnimation();
            }
        }
        else
        {
            if (toggle)
            {
                PlayAnimation();
            }
        }
        
    }

    private void PlayAnimation()
    {
        var tween = GetTree().CreateTween();
        var (propertyName, value) = _propertyMap[PropertyTween]();
        tween.SetEase(TweenEaseType);
        tween.TweenProperty(ParentControl, propertyName, value, _tweenDuration);
        if (_reverseAnim && Trigger != TriggerEnum.FocusEntered && Trigger != TriggerEnum.FocusExited
            && Trigger != TriggerEnum.VisibilityChanged && Trigger != TriggerEnum.MouseEntered && Trigger != TriggerEnum.MouseExited)
        {
            tween.Finished += PlayReverseAnimation;
        }
    }

    private void PlayReverseAnimation()
    {
        var tween = GetTree().CreateTween();
        tween.SetEase(TweenEaseType);
        var (propertyName, value) = _propertyMap[PropertyTween]();
        tween.TweenProperty(ParentControl, propertyName, _originalValues[PropertyTween], _reverseTweenDuration);
    }

    private void CenterPivot()
    {
        ParentControl.PivotOffset = ParentControl.Size / 2;
    }
}
