using Godot;
using System;
using static SignalBus;
/// <summary>
/// The UI element for displaying a music entry in the playlist. Contains the information of its corresponding <c>MusicResource</c> and emits a signal with the resource when double-clicked, which is used by <c>AudioManager</c> to play the selected track.
/// </summary>
public partial class MusicEntry : BoxContainer
{
    /// <summary>
    /// The <c>MusicResource</c> that this entry represents.
    /// </summary>
    public MusicResource MusicResource { get; set; }
    /// <summary>
    /// Label for displaying the track name, styled with the "MusicEntryLabel" theme type variation by default and "CurrentSongEntryLabel" when the entry is for the currently playing track.
    /// </summary>
    private Label _nameLabel;
    /// <summary>
    /// Label for displaying the track artist, styled with the "MusicEntryLabel" theme type variation by default and "CurrentSongEntryLabel" when the entry is for the currently playing track.
    /// </summary>
    private Label _artistLabel;
    /// <summary>
    /// Label for displaying the track album name, styled with the "MusicEntryLabel" theme type variation by default and "CurrentSongEntryLabel" when the entry is for the currently playing track.
    /// </summary>
    private Label _albumLabel;

    public override void _Ready()
    {
        _nameLabel = GetNode<Label>("SongNameLabel");
        _artistLabel = GetNode<Label>("ArtistLabel");
        _albumLabel = GetNode<Label>("AlbumLabel");
        
        _nameLabel.Text = MusicResource.Name;
        _artistLabel.Text = MusicResource.Artist;
        _albumLabel.Text = MusicResource.Album;
        
        _nameLabel.TooltipText = MusicResource.Name;
        _artistLabel.TooltipText = MusicResource.Artist;
        _albumLabel.TooltipText = MusicResource.Album;
    }
    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.DoubleClick)
        {
            SigBus.EmitSignal(nameof(SigBus.MusicEntrySelected), MusicResource);
        }
    }

    public void ChangeLabels(bool isCurrent)
    {
        if (isCurrent)
        {
            _nameLabel.ThemeTypeVariation = "CurrentSongEntryLabel";
            _artistLabel.ThemeTypeVariation = "CurrentSongEntryLabel";
            _albumLabel.ThemeTypeVariation = "CurrentSongEntryLabel";
        }
        else
        {
            _nameLabel.ThemeTypeVariation = "MusicEntryLabel";
            _artistLabel.ThemeTypeVariation = "MusicEntryLabel";
            _albumLabel.ThemeTypeVariation = "MusicEntryLabel";
        }
    }
}
