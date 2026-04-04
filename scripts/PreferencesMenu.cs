using Godot;
using System;
using static Global;

public partial class PreferencesMenu : Window
{
    [Export]
    private LineEdit _ffmpegPath;
    [Export]
    private LineEdit _defaultDirectory;
    [Export]
    private OptionButton _scaleButton;
    [Export]
    private OptionButton _sortingButton;
    [Export]
    private OptionButton _audioOutputButton;
    private Vector2 _visibleMousePosition;
    private ConfigFile _configFile;
    private PanelContainer _rootContainer;
    public override void _Ready()
    {
        VisibilityChanged += OnVisible;
        _rootContainer = GetNode<PanelContainer>("PanelContainer");
        SetScaleZero();
        LoadPreferences();
    }

    private void OnVisible()
    {
        if (!Visible) return;
        SetPosition();
        SetupAudioDevice();
    }

    private void ExitButtonPressed()
    {
        Visible = false;
        var time = GetTree().CreateTimer(0.2f);
        time.Timeout += SetScaleZero;
    }

    private void SetScaleZero()
    {
        _rootContainer.SetScale(new Vector2I(0, 0));
    }

    private void LoadPreferences()
    {
        _configFile = new ConfigFile();
        var err = _configFile.Load("user://pref.cfg");
        if (err != Error.Ok)
        {
            return;
        }
        SetResolution((int)_configFile.GetValue("Preferences", "Scale", 0));
        Instance.FirstDirectoryPath = (string)_configFile.GetValue("Preferences", "DefaultDirectory","C:/");
        _defaultDirectory.Text = (string)_configFile.GetValue("Preferences", "DefaultDirectory", "C:/");
        _defaultDirectory.TooltipText = _defaultDirectory.Text;
        var musicSort = (bool)_configFile.GetValue("Preferences", "DefaultSort", false);
        Instance.MusicListAlphabeticalSort = musicSort;
        Instance.FfmpegPath = (string)_configFile.GetValue("Preferences", "FfmpegPath", "/usr/bin/ffmpeg");
        _ffmpegPath.Text = Instance.FfmpegPath;
        _ffmpegPath.TooltipText = _ffmpegPath.Text;
        _sortingButton.Selected = musicSort ? 0 : 1;
        SetupAudioDevice();
    }
    private void SetPosition()
    {
        Position = GetTree().GetRoot().Position;
    }
    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if(@event is InputEventMouseMotion mouseMotion && Input.IsMouseButtonPressed(MouseButton.Right))
        {
            if (Input.MouseMode == Input.MouseModeEnum.Visible)
            {
                _visibleMousePosition = GetViewport().GetMousePosition();
                Input.MouseMode = Input.MouseModeEnum.Captured;
            }
            Position += (Vector2I)mouseMotion.Relative;
        }

        if (Input.MouseMode == Input.MouseModeEnum.Captured && !Input.IsMouseButtonPressed(MouseButton.Right))
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
            GetViewport().WarpMouse(_visibleMousePosition);
            var testEvent = new InputEventMouseMotion();
            testEvent.GlobalPosition = _visibleMousePosition;
            Input.ParseInputEvent(testEvent);
        }
    }

    private void SetResolution(int scale)
    {
        _configFile.SetValue("Preferences", "Scale", scale);
        _scaleButton.Select(scale);
        switch (scale)
        {
            case 0:
                DisplayServer.WindowSetSize(new Vector2I(600, 400), 0);
                GetTree().GetRoot().ContentScaleFactor = 1.0f;
                ContentScaleFactor = 1.0f;
                Size = new Vector2I(400, 300);
                break;
            case 1:
                DisplayServer.WindowSetSize(new Vector2I(900, 600), 0);
                Size = new Vector2I(600, 450);
                ContentScaleFactor = 1.5f;
                GetTree().GetRoot().ContentScaleFactor = 1.5f;
                break;
            case 2:
                DisplayServer.WindowSetSize(new Vector2I(1200, 800), 0);
                Size = new Vector2I(800, 600);
                ContentScaleFactor = 2.0f;
                GetTree().GetRoot().ContentScaleFactor = 2.0f;
                break;
        }
    }

    private void SortingButtonPressed(int sortingOrder)
    {
        Instance.MusicListAlphabeticalSort = (sortingOrder == 0);
        _configFile.SetValue("Preferences", "DefaultSort", Instance.MusicListAlphabeticalSort);
    }

    private void DirectoryLineEditChanged(string newText)
    {
        Instance.FirstDirectoryPath = ProjectSettings.GlobalizePath(newText) + "/";
        _configFile.SetValue("Preferences", "DefaultDirectory", Instance.FirstDirectoryPath);
        _defaultDirectory.Text = Instance.FirstDirectoryPath;
        _defaultDirectory.TooltipText = _defaultDirectory.Text;
    }
    private void FfmpegDirectoryLineEditChanged(string newText)
    {
        Instance.FfmpegPath = ProjectSettings.GlobalizePath(newText);
        _configFile.SetValue("Preferences", "FfmpegPath", Instance.FfmpegPath);
        _ffmpegPath.Text = Instance.FfmpegPath;
        _ffmpegPath.TooltipText = _ffmpegPath.Text;
    }

    private void SetupAudioDevice()
    {
        string[] deviceList = AudioServer.GetOutputDeviceList();
        _audioOutputButton.Clear();
        for(var i = 0; i<deviceList.Length; i++)
        {
            _audioOutputButton.AddItem(deviceList[i]);
            if (deviceList[i] == (string)_configFile.GetValue("Audio", "OutputDevice", "Default"))
            {
                _audioOutputButton.Select(i);
            }
        }
        if (_audioOutputButton.Selected == -1) // if saved audio device doesnt exist
        {
            _audioOutputButton.Select(0); // select default
        }
        _configFile.SetValue("Audio", "OutputDevice", _audioOutputButton.GetItemText(_audioOutputButton.Selected));
    }

    private void AudioOutputSelected(int index)
    {
        var deviceName = _audioOutputButton.GetItemText(index);
        AudioServer.SetOutputDevice(deviceName);
        _configFile.SetValue("Audio", "OutputDevice", deviceName);
    }

    private void SavePreferences()
    {
        _configFile.Save("user://pref.cfg");
        Visible = false;
    }
    
}
