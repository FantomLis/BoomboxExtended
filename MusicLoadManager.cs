using System;
using BepInEx;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using pworld.Scripts.Extensions;
using UnityEngine;
using UnityEngine.Networking;

namespace FantomLis.BoomboxExtended
{
    public class MusicManager : MonoBehaviour
    {
        private static MusicManager instance;
        public static Dictionary<string, AudioClip> AudioClips = new ();

        protected static new Coroutine StartCoroutine(IEnumerator enumerator)
        {
            if (instance == null)
            {
                instance = new GameObject("MusicLoader").AddComponent<MusicManager>();
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
                            byte[] fileData = ClipsToByte(clip); 
                            Boombox.log.LogInfo(SHA256FromBytes(fileData));
                            clip.name = Path.GetFileName(file) + SHA256FromBytes(fileData).Substring(0, 16);
                            AudioClips.Add(file + SHA256FromBytes(fileData).Substring(0, 16),clip);

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

        public static byte[] ClipsToByte(AudioClip clip)
        {
            float[] _a = new float[clip.samples];
            clip.GetData(_a, 0);
            byte[] fileData = new byte[_a.Length*4];
            Buffer.BlockCopy(_a, 0, fileData, 0, fileData.Length);
            return fileData;
        }
        
        private static string SHA256FromBytes(byte[] data)
        {
            var crypt = new SHA256Managed();
            string hash = String.Empty;
            byte[] crypto = crypt.ComputeHash(data);
            foreach (byte theByte in crypto)
            {
                hash += theByte.ToString("x2");
            }
            return hash;
        }
    }
}
