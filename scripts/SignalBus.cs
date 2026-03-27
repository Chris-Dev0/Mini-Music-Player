using Godot;
using System;
/// <summary>
/// A singleton node that serves as a global signal bus for different parts of the app.
/// </summary>
public partial class SignalBus : Node
{
    /// <summary>
    /// Static reference to the SignalBus instance.
    /// </summary>
    public static SignalBus SigBus { get; private set; }
    /// <summary>
    /// A signal that tells the UiManager to create a new notification with the given parameters.
    /// </summary>
    /// <param name="type">0 = normal, 1 = error</param>
    /// <param name="message">message text</param>
    /// <param name="duration">duration in seconds</param>
    [Signal]
    public delegate void SendNotificationEventHandler(int type, string message,float duration);
    /// <summary>
    /// A signal to notify when a new directory has been selected in the file dialog.
    /// </summary>
    /// <param name="directory"></param>
    [Signal]
    public delegate void NewDirectorySelectedEventHandler(string directory);
    /// <summary>
    /// A signal emitted when a music entry is selected in the UI.
    /// </summary>
    /// <param name="resource"></param>
    [Signal]
    public delegate void MusicEntrySelectedEventHandler(MusicResource resource);
    /// <summary>
    /// A signal emitted when the current song changes.
    /// </summary>
    /// <param name="resource"></param>
    [Signal]
    public delegate void SongChangedEventHandler(MusicResource resource);
    public override void _Ready()
    {
        base._Ready();
        SigBus = this;
    }
}
