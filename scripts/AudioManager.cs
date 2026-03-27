using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static SignalBus;
using static Global;
/// <summary>
/// Handles audio playback and manages the song queue.
/// </summary>
public partial class AudioManager : Node
{
    [Export]
    private Button _playPauseButton;
    [Export]
    private Button _skipBackButton;
    [Export] 
    private Button _skipForwardButton;
    [Export]
    private Button _trackRepeatButton;
    [Export]
    private Button _shuffleButton;

    [Export]
    private Texture2D _trackNoRepeatIcon;
    [Export]
    private Texture2D _trackSingleRepeatIcon;
    [Export]
    private Texture2D _trackRepeatIcon;

    [Export]
    private Texture2D _playButtonTexture;
    [Export]
    private Texture2D _pauseButtonTexture;
    [Export]
    private Texture2D _shuffleButtonTexture;
    [Export]
    private Texture2D _noShuffleButtonTexture;
    
    private AudioStreamPlayer _player;
    private readonly List<MusicResource>  _queue = new();
    private bool _shuffleToggle;
    private MusicResource _nextSong;
    private MusicResource _currentSong;
    private int _queueIndex;
    private readonly Random _random = new();
    private FileManager _fileManager;
    private enum TrackRepeat
    {
        NoRepeat,
        SingleTrackRepeat,
        PlaylistRepeat
    }
    private TrackRepeat _currentTrackRepeat;
    

    public override void _Ready()
    {
        _player = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
        SigBus.MusicEntrySelected += MusicEntrySelected;
        _currentTrackRepeat = TrackRepeat.NoRepeat;
        _shuffleToggle = false;
        _fileManager = Global.FileManagerInstance;
    }
    /// <summary>
    /// Clear the queue and stop the music when a new directory is selected.
    /// </summary>
    private void NewDirectorySelected()
    {
        _player.Stream = null;
        _currentSong = null;
        _queueIndex = 0;
        _queue.Clear();
    }
    
    /// <summary>
    /// When a music entry is selected from the song list, create a new queue with <see cref="SetupQueue"/> and start playing the new song
    /// </summary>
    /// <param name="resource"></param>
    private void MusicEntrySelected(MusicResource resource)
    {
        SetupQueue(resource);
        SongChanged(resource);
        _playPauseButton.Icon = _pauseButtonTexture;
    }
    /// <summary>
    /// Sets up the song queue based on the selected song, current shuffle and repeat settings, and the global music resource list. Called when a new song is selected from the song list or when skip forward/backward is pressed.
    /// </summary>
    /// <param name="resource">The first/current song in the queue</param>

    private void SetupQueue(MusicResource resource)
    {
            var startIndex = Instance.MusicResources.IndexOf(resource);
            _currentSong = resource;
            _queue.Clear();
            _queue.AddRange(Instance.MusicResources.GetRange(startIndex, Instance.MusicResources.Count - startIndex));
            if (_currentTrackRepeat == TrackRepeat.PlaylistRepeat && startIndex > 0 || _shuffleToggle)
            {
                _queue.AddRange(Instance.MusicResources.GetRange(0, startIndex));
                if (_shuffleToggle)
                {
                    var shuffledQueue = _queue.OrderBy(_ => _random.Next()).ToList();
                    _queue.Clear();
                    _queue.AddRange(shuffledQueue);
                    _queue.Remove(resource);
                    _queue.Insert(0, resource);
                }
            }
            _queueIndex = _queue.IndexOf(resource);
            SetNextSong();
    }
    /// <summary>
    /// Determines the next song to play based on the current song, shuffle and repeat settings, and the song queue. Called from <c>SetupQueue</c> and when a song finishes playing.
    /// </summary>
    private void SetNextSong()
    {
        if (_currentTrackRepeat == TrackRepeat.SingleTrackRepeat)
        {
            _nextSong = _currentSong;
        }
        else
        {
            if (_queueIndex+1 < _queue.Count)
            {
                _nextSong = _queue[_queueIndex+1];
            }
            else if (_currentTrackRepeat == TrackRepeat.PlaylistRepeat)
            {
                SetupQueue(_queue[_queueIndex+1]);
            }
            else
            {
                _nextSong = null;
            }
        }
    }
    /// <summary>
    /// Plays the next song in the queue when the current song finishes, or when skip forward is pressed.
    /// </summary>
    private void PlayNextSong()
    {
        if (_nextSong == null) return;
        SongChanged(_nextSong);
        _queueIndex = _queue.IndexOf(_nextSong);
        SetNextSong();
    }
    
    /// <summary>
    /// Handles changing the current song, including loading the new song into the AudioStreamPlayer, starting playback, and emitting the SongChanged signal.
    /// </summary>
    /// <param name="resource"></param>
    private void SongChanged(MusicResource resource)
    {
        _player.Stream=_fileManager.LoadMusicResource(resource.Path);
        _player.Seek(0.0f);
        _player.Play();
        _player.Seek(0.0f);
        _playPauseButton.Icon = _pauseButtonTexture;
        _playPauseButton.TooltipText = "Pause";
        SigBus.EmitSignal(nameof(SigBus.SongChanged),resource);
        
    }
    /// <summary>
    /// Handles play/pause button presses, toggling playback of the current song and updating the button icon and tooltip.
    /// </summary>
    private void _playPauseButtonPressed()
    {
        if (_player.Stream == null) return;
        if (_player.IsPlaying())
        {
            _player.StreamPaused = true;
            _playPauseButton.TooltipText = "Play";
            _playPauseButton.Icon = _playButtonTexture;
        }
        else
        {
            _player.StreamPaused = false;
            _playPauseButton.TooltipText = "Pause";
            _playPauseButton.Icon = _pauseButtonTexture;
        }
        
    }
    private void _skipBackButtonPressed()
    {
        if (_player.Stream == null) return;
        if (_player.GetPlaybackPosition() > 1 )
        {
            _player.Seek(0.0f);
        }
        else
        {
            if (_currentTrackRepeat is TrackRepeat.NoRepeat or TrackRepeat.PlaylistRepeat && Instance.MusicResources.IndexOf(_currentSong) != 0)
            {
                SetupQueue(Instance.MusicResources[Instance.MusicResources.IndexOf(_currentSong)-1]);
                SongChanged(_currentSong);
            }
            else if (_currentTrackRepeat == TrackRepeat.PlaylistRepeat && _queueIndex == 0)
            {
                SetupQueue(_queue[_queue.Count - 1]);
                SongChanged(_currentSong);
            }
            else if (_currentTrackRepeat == TrackRepeat.SingleTrackRepeat)
            {
                SetNextSong();
                PlayNextSong();
            }
        }
    }
    private void _skipForwardButtonPressed()
    {
        if (_player.Stream == null) return;
        PlayNextSong();
        if (_currentTrackRepeat == TrackRepeat.SingleTrackRepeat)
        {
            SetupQueue(_currentSong);
        }
        else
        {
            if (_queueIndex < _queue.Count - 1)
            {
                SetupQueue(_queue[_queueIndex]);
            }
        }
    }
    private void _trackRepeatButtonPressed()
    {
        switch (_currentTrackRepeat)
        {
            case TrackRepeat.NoRepeat:
                _currentTrackRepeat = TrackRepeat.SingleTrackRepeat;
                _trackRepeatButton.Icon = _trackSingleRepeatIcon;
                SetNextSong();
                break;
            case TrackRepeat.SingleTrackRepeat:
                _currentTrackRepeat = TrackRepeat.PlaylistRepeat;
                _trackRepeatButton.Icon = _trackRepeatIcon;
                break;
            case TrackRepeat.PlaylistRepeat:
                _currentTrackRepeat = TrackRepeat.NoRepeat;
                _trackRepeatButton.Icon = _trackNoRepeatIcon;
                break;
        }
        if (_currentSong != null)
        {
            SetupQueue(_currentSong);
        }
    }
    private void _shuffleButtonPressed()
    {
        _shuffleButton.Icon = _shuffleToggle ? _noShuffleButtonTexture : _shuffleButtonTexture;
        _shuffleToggle = !_shuffleToggle;
        if (_player.Stream != null)
        {
            SetupQueue(_currentSong);
        }
    }
}
