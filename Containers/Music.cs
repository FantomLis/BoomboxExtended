using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using FantomLis.BoomboxExtended.Locales;
using FantomLis.BoomboxExtended.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace FantomLis.BoomboxExtended.Containers;

public record Music
{
    public string FilePath
    {
        get => __FilePath;
        set
        {
            if (__FilePath != value) {
            {
                Clip?.UnloadAudioData();
                Clip = null;
                __FilePath = value;
            }} 
        } }

    private string __FilePath;
    public string Name => Path.GetFileNameWithoutExtension(FilePath);
    public bool isLoaded => Clip != null && Clip.loadState == AudioDataLoadState.Loaded;
    public AudioClip? Clip;

    public Music(string path, AudioClip clip)
    {
        FilePath = path;
        Clip = clip;
    }

    public Music(string path) => FilePath = path;

    public async Task<bool> LoadMusic()
    {
        try
        {
            AudioType type = MusicLoadManager.GetAudioType(FilePath);
            if (type == AudioType.UNKNOWN) return false;
            UnityWebRequest loader = UnityWebRequestMultimedia.GetAudioClip(FilePath, type);
            DownloadHandlerAudioClip handler = (DownloadHandlerAudioClip)loader.downloadHandler;
            handler.streamAudio = false;

            loader.SendWebRequest();
            await Task.Run(() =>
            {
                while (!handler.isDone) ;
            });
            if (loader.error == null)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(loader);
                if (clip && clip.loadState == AudioDataLoadState.Loaded && clip.length != 0)
                {
                    clip.name = Path.GetFileNameWithoutExtension(FilePath);
                    Clip = clip;
                    if (!isLoaded) throw new InvalidOperationException("File loading failed with unknown reason");
                }
            }
            else
            {
                throw new FileLoadException(loader.error);
            }

            return true;
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}