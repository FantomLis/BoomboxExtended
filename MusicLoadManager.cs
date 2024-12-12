using BepInEx;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace FantomLis.BoomboxExtended
{
    public class MusicLoadManager : MonoBehaviour
    {
        private static MusicLoadManager instance;
        public static Dictionary<string, AudioClip> AudioClips = new ();

        protected static new Coroutine StartCoroutine(IEnumerator enumerator)
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
        
        
        /// <summary>
        /// Loads music from folder
        /// </summary>
        /// <param name="pathToFolder">Path to folder to load music (default: "Custom Song", if player is host)</param>
        /// <param name="resetLoadedClips">Clears loaded clips</param>
        public static IEnumerator LoadMusic(string pathToFolder = "Custom Song", bool resetLoadedClips = true)
        {
            if (resetLoadedClips) AudioClips.Clear();
            string path = Path.Combine(Paths.PluginPath, pathToFolder);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            foreach (string file in Directory.GetFiles(path))
            {
                if (AudioClips.ContainsKey(file)) continue; 
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
                            clip.name = Path.GetFileName(file);
                            AudioClips.Add(file,clip);

                            Boombox.log.LogInfo($"Music Loaded: {clip.name}");
                        }
                    }
                }
            }
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
