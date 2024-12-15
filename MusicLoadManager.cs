using System;
using BepInEx;
using System.Collections;
using System.IO;
using MyceliumNetworking;
using UnityEngine;
using UnityEngine.Networking;

namespace FantomLis.BoomboxExtended
{
    public class MusicLoadManager : MonoBehaviour
    {
        private static MusicLoadManager instance;

        public static new Coroutine StartCoroutine(IEnumerator enumerator)
        {
            if (instance == null)
            {
                instance = new GameObject("MusicLoader").AddComponent<MusicLoadManager>();
                DontDestroyOnLoad(instance);
            }

            return ((MonoBehaviour)instance).StartCoroutine(enumerator);
        }

        public static void StartLoadMusic()
        {
            StartCoroutine(LoadMusic());
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
        public static IEnumerator LoadMusic()
        {
            string path = Path.Combine(Paths.GameRootPath, "Plugins/Boombox", "Custom Songs");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            BoomboxBehaviour.clips.Clear();
            Boombox .DropQueuedAlert("Loaded music");
            foreach (string file in Directory.GetFiles(path))
            {
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
                            if (BoomboxBehaviour.clips.ContainsKey(file)) continue;
                            clip.name = Path.GetFileName(file);
                            BoomboxBehaviour.clips.Add(clip.name,clip);

                            Boombox.log.LogInfo($"Music Loaded: {clip.name}");
                            Boombox.ShowRevenueAlert("Loaded music", clip.name);
                        }
                    }
                }
            }
            Boombox.log.LogInfo($"Music loading finished!");
            Boombox.ShowRevenueAlert("Loading music finished!", $"Loaded {BoomboxBehaviour.clips.Count.ToString()} tracks", dropQueuedAlert:true);
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
