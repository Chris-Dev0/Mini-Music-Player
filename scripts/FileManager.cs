using Godot;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using static Global;
using static SignalBus;
using System.Diagnostics;
using System.IO;
/// <summary>
/// Handles directory selection and loading music files into the application as MusicResource objects.
/// </summary>
public partial class FileManager : Node
{
    [Export]
    private Texture _defaultAlbumArtTexture;
    private FileDialog _fileDialog;
    private string _lastDirectoryPath;
    private bool _firstDirectory = true;
    private UiManager _uiManagerInstance;
    public override void _Ready()
    {
        _fileDialog = GetNode<FileDialog>("FileDialog");
        _fileDialog.DirSelected += NewDirectorySelected;
        _lastDirectoryPath = "";
        _uiManagerInstance = Global.UiManagerInstance;
    }
    /// <summary>
    /// Loads music from the selected directory and tells the UI manager to update the music list. Called when a new directory is selected in the file dialog.
    /// </summary>
    /// <param name="directory"></param>
    private async void NewDirectorySelected(string directory)
    {
        _firstDirectory = false;
        _lastDirectoryPath = directory;
        SigBus.EmitSignal(nameof(SignalBus.SendNotification), 0, "Fetching music files...", 1.5);
        var result = await Task.Run(() => GetMusicFiles(directory));
        _uiManagerInstance.Call("PopulateMusicList");
    }
    /// <summary>
    /// Scans the selected directory for music files, creates MusicResource objects for each file with metadata via taglib, and adds them to the global music resource list. Called from NewDirectorySelected.
    /// </summary>
    /// <param name="directory"></param>
    /// <returns></returns>
    private async Task<int> GetMusicFiles(string directory)
    {
        var openDir = DirAccess.Open(directory);
        if (openDir != null)
        {
            Instance.MusicResources.Clear();
            openDir.SetIncludeNavigational(false);
            openDir.SetIncludeHidden(false);
            openDir.ListDirBegin();
            var filename = openDir.GetNext();
            while (filename != "" && filename != "." && filename != "..")
            {
                if (filename.GetExtension() == "mp3" || filename.GetExtension() == "ogg" ||
                    filename.GetExtension() == "wav" || filename.GetExtension() == "flac")
                {
                    var fixedDir = ProjectSettings.GlobalizePath(directory + "/" + filename);
                    using var tagFile = TagLib.File.Create(fixedDir);
                    var music = new MusicResource
                    {
                        Path = directory + "/" + filename,
                        Name = tagFile.Tag.Title ?? System.IO.Path.GetFileNameWithoutExtension(filename),
                        Artist = tagFile.Tag.FirstPerformer ?? "unknown",
                        Album = tagFile.Tag.Album ?? "unknown",
                        TrackNumber = (int)tagFile.Tag.Track
                    };
                    switch (filename.GetExtension())
                    {
                        case "mp3":
                            music.Extension = "mp3";
                            break;
                        case "ogg":
                            music.Extension = "ogg";
                            break;
                        case "wav":
                            music.Extension = "wav";
                            break;
                        case "flac":
                            music.Extension = "flac";
                            break;
                    }
                    Instance.MusicResources.Add(music);
                }
                filename = openDir.GetNext();
            }
        }
        else
        {
            SigBus.EmitSignal(nameof(SigBus.SendNotification), 1, "Directory is not valid, " + DirAccess.GetOpenError(), 2);
        }
        if (!Instance.MusicListAlphabeticalSort)
            Instance.MusicResources.Sort((resource, musicResource) => resource.TrackNumber.CompareTo(musicResource.TrackNumber));
        else
            Instance.MusicResources.Sort((resource, musicResource) => resource.Name.CompareTo(musicResource.Name));
        return 0;
    }
    /// <summary>
    /// Called from AudioManager to load the next music resource to be played.
    /// </summary>
    /// <param name="filepath"></param>
    public AudioStream LoadMusicResource(string path)
    {
        using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
        if (file == null)
        {
            GD.PushError($"Could not open file {path}");
            return null;
        }
        switch (path.GetExtension())
        {
            case "mp3":
                var sound = new AudioStreamMP3();
                sound.Data = file.GetBuffer((long)file.GetLength());
                return sound;
            case "wav":
                var wavSound = new AudioStreamWav();
                wavSound.Data = file.GetBuffer((long)file.GetLength());
                return wavSound;
            case "ogg":
                var oggSound = AudioStreamOggVorbis.LoadFromFile(path);
                return oggSound;
            case "flac":
                var oggData = ConvertToOggInMemory(path);
                return AudioStreamOggVorbis.LoadFromBuffer(oggData);

        }
        return null;
    }
    public static byte[] ConvertToOggInMemory(string inputPath)
    {
        var convertProcess = new ProcessStartInfo
        {
            FileName = Instance.FfmpegPath,
            Arguments = $"-i \"{inputPath}\" -f ogg -acodec libvorbis pipe:1",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(convertProcess);
        using var ms = new MemoryStream();
        process.StandardOutput.BaseStream.CopyTo(ms);
        process.WaitForExit();
        return ms.ToArray();
    }
    /// <summary>
    /// Called from the AudioManager to load album art for the currently playing song. If no album art is found, the default album art texture is used.
    /// </summary>
    /// <param name="resource"></param>
    public void LoadAlbumArt(MusicResource resource)
    {
        using var tagFile = TagLib.File.Create(resource.Path);
        var pictureData = tagFile.Tag.Pictures.Length > 0 ? tagFile.Tag.Pictures[0].Data.Data : null;
        if (pictureData != null)
        {
            var albumImage = new Image();
            var mimeType = tagFile.Tag.Pictures[0].MimeType.ToLower();
            if (mimeType.Contains("jpeg") || mimeType.Contains("jpg"))
            {
                albumImage.LoadJpgFromBuffer(pictureData);
                //albumImage.Resize(200, 200, Image.Interpolation.Lanczos);
                resource.AlbumArt = ImageTexture.CreateFromImage(albumImage);
            }
            else if (mimeType.Contains("png"))
            {
                albumImage.LoadPngFromBuffer(pictureData);
                //albumImage.Resize(200, 200, Image.Interpolation.Lanczos);
                resource.AlbumArt = ImageTexture.CreateFromImage(albumImage);
            }
            else
            {
                resource.AlbumArt = _defaultAlbumArtTexture;
            }
        }

    }

    /// <summary>
    /// Shows the native OS file dialog for selecting a directory to load music from, starting at the last selected directory or a default path if the last directory is not valid.
    /// </summary>
    private void ShowFileDialog()
    {
        if ((_firstDirectory && DirAccess.DirExistsAbsolute(Instance.FirstDirectoryPath)) || (!DirAccess.DirExistsAbsolute(_lastDirectoryPath)))
        {
            _fileDialog.SetCurrentPath(Instance.FirstDirectoryPath);
        }
        else if (!DirAccess.DirExistsAbsolute(_lastDirectoryPath))
        {
            _fileDialog.SetCurrentPath("C:/"); //add linux fallback path
        }
        else
        {
            _fileDialog.SetCurrentPath(_lastDirectoryPath);
        }
        _fileDialog.SetVisible(true);
    }
}

