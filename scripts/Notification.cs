using Godot;
using System;
/// <summary>
/// Notification class used for displaying short temp messages in the user interface. Can be created using the <c>SendNotificationEventHandler</c> signal in <c>SignalBus</c>.
/// </summary>
public partial class Notification : MarginContainer
{
    /// <summary>
    /// Timer for how long the notification should be displayed, after which it will automatically free itself.
    /// </summary>
    private Timer _timer;
    /// <summary>
    /// Label for displaying the notification message, can be styled using the "ErrorLabel" theme type variation for error messages (type 1).
    /// </summary>
    private Label _label;
    /// <summary>
    /// AnimationPlayer for playing the notification's entrance animation when created.
    /// </summary>
    private AnimationPlayer _player;
    public override void _Ready()
    {
        _timer = GetNode<Timer>("Timer");
        _label = GetNode<Label>("PanelContainer/MarginContainer/Label");
        _player = GetNode<AnimationPlayer>("AnimationPlayer");

        _timer.Timeout += QueueFree;
        MinimumSizeChanged += SetPivot;
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if(Input.IsMouseButtonPressed(MouseButton.Left))
        {
            QueueFree();
        }
    }

    private void SetPivot()
    {
        var newSize = new Vector2(0, 0);
        PivotOffset = newSize;
    }
    /// <summary>
    /// Sets the parameters for the notification, including the type (for styling), message, and duration before removal. Also starts the animation.
    /// </summary>
    /// <param name="type">0, for normal messages; 1, for errors</param>
    /// <param name="message"></param>
    /// <param name="duration"></param>
    public void SetParams(int type, string message, float duration)
    {
        _label.Text = message;
        if (type == 1)
        {
            _label.ThemeTypeVariation = "ErrorLabel";
        }
        _timer.SetWaitTime(duration);
        _timer.Start();
        _player.Play("ScaleUp");
    }
}
