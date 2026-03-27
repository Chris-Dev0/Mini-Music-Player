using Godot;
using System;
/// <summary>
/// A resource class for music tracks, contains metadata and the path to the track file.
/// </summary>
[GlobalClass]
public partial class MusicResource : Resource
{
    /// <summary>
    /// The file path to the music track, set when music is loaded from the chosen directory by <c> Global.Instance.FileManageInstancer</c>.
    /// </summary>
    [Export]
    public string Path { get; set; }
    /// <summary>
    /// The name of the track.
    /// </summary>
    [Export]
    public string Name { get; set; }
    /// <summary>
    /// The track artist.
    /// </summary>
    [Export]
    public string Artist { get; set; }
    /// <summary>
    /// The album the music track is from.
    /// </summary>
    [Export]
    public string Album { get; set; }
    /// <summary>
    /// The album art for the music track.
    /// </summary>
    [Export]
    public Texture AlbumArt { get; set; }
    /// <summary>
    /// The track number on the album, used for sorting when <c> Global.Instance.MusicListAlphabeticalSort</c> is false.
    /// <remarks> Set to 0 if track number is not found in file metadata </remarks>
    /// </summary>
    [Export]
    public int TrackNumber { get; set; }
    /// <summary>
    /// The file extension of the music track.
    /// </summary>
    [Export]
    public string Extension { get; set; }
}
