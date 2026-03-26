using Godot;
using System;
using System.Collections.Generic;

public partial class Global : Node
{
    public static Global Instance { get; private set; }
    public readonly List<MusicResource> MusicResources = new();
    public bool MusicListAlphabeticalSort = false;
    public static FileManager FileManagerInstance => Instance.GetNode<FileManager>("/root/Main/FileManager");
    public string FirstDirectoryPath;
    public override void _Ready()
    {
        base._Ready();
        Instance = this;
    }
}
