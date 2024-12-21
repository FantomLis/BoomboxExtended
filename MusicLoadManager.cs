using System;
using BepInEx;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using FantomLis.BoomboxExtended.Utils;
using MyceliumNetworking;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Networking;

namespace FantomLis.BoomboxExtended
{
    public class MusicLoadManager : MonoBehaviour
    {
        private static MusicLoadManager Instance;
        public static Dictionary<string, AudioClip> clips = new ();

        /// <summary>
        /// Default Root Path from which music is searching
        /// </summary>
        public static string RootPath = Path.Combine(Application.dataPath, "Mod Resources", "Boombox");
        private new static Coroutine StartCoroutine(IEnumerator enumerator)
        {
            Instance ??= GameObjectUtils.MakeNewDontDestroyOnLoad("MusicLoader").AddComponent<MusicLoadManager>();
            return ((MonoBehaviour)Instance).StartCoroutine(enumerator);
        }

        /// <summary>
        /// Loads music from folder
        /// </summary>
        /// <param name="rootPath">Root path from which music is searching (null = <see cref="MusicLoadManager.RootPath"/>)</param>
        /// <param name="musicPath">Path to music folder in Root</param>
        /// <param name="reloadAllSongs">Fully reload all songs</param>
        public static void StartLoadMusic(string? rootPath = null, string musicPath = "Custom Songs", bool reloadAllSongs = true)
        {
            rootPath ??= RootPath;
            string path = System.IO.Path.Combine(rootPath, musicPath);
            if (reloadAllSongs) clips.Clear();
            StartCoroutine(LoadMusic(path));
        }
        
        /*
        /// <summary>
        /// Loads music from folder
        /// </summary>
        /// <param name="pathToFolder">Path to folder to load music (default: "Custom Song", if player is host)</param>
        */
        /*public static IEnumerator LoadMusic(string pathToFolder = "Custom Song")
        {
            string path = Path.Combine(Paths.PluginPath, pathToFolder);*/
        /*if (BoomboxBehaviour.clips.ContainsKey(file)) continue;*/
        public static IEnumerator LoadMusic(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            AlertUtils.DropQueuedMoneyCellAlert("Loaded music");
            foreach (string file in Directory.GetFiles(path))
            {
                if (clips.ContainsKey(Path.GetFileNameWithoutExtension(file))) continue;
                AudioType type = GetAudioType(file);
                if (type != AudioType.UNKNOWN)
                {
                    UnityWebRequest loader = UnityWebRequestMultimedia.GetAudioClip(file, type);

                    DownloadHandlerAudioClip handler = (DownloadHandlerAudioClip)loader.downloadHandler;
                    handler.streamAudio = false;

                    loader.SendWebRequest();

                    yield return new WaitUntil(() => loader.isDone);

                    if (loader.error == null)
                    {
                        AudioClip clip = DownloadHandlerAudioClip.GetContent(loader);
                        if (clip && clip.loadState == AudioDataLoadState.Loaded)
                        {
                            clip.name = Path.GetFileNameWithoutExtension(file);
                            clips.Add(clip.name,clip);

                            LogUtils.LogInfo($"Music Loaded: {clip.name}");
                            AlertUtils.AddMoneyCellAlert("Loaded music", MoneyCellUI.MoneyCellType.Revenue, clip.name);
                        }
                    }
                }
            }
            LogUtils.LogInfo($"Music loading finished!");
            AlertUtils.AddMoneyCellAlert("Loading music finished!", MoneyCellUI.MoneyCellType.MetaCoins, $"Loaded {clips.Count.ToString()} tracks", dropQueuedAlert:true);
        }

        private static AudioType GetAudioType(string path)
        {
            var extension = Path.GetExtension(path).ToLower();

            if (extension == ".wav")
                return AudioType.WAV;
            if (extension == ".ogg")
                return AudioType.OGGVORBIS;
            if (extension == ".mp3")
                return AudioType.MPEG;

            return AudioType.UNKNOWN;
        }
    }
}
