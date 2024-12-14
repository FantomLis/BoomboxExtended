using System;
using BepInEx;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Networking;

namespace FantomLis.BoomboxExtended
{
    public class MusicLoadManager : MonoBehaviour
    {
        private static MusicLoadManager instance;
        public static Dictionary<string, AudioClip> AudioClips = new ();
        private static readonly string _music_hash_splitter_replacer = "\'/\'/\'";
        public static int ChunkSize = 1024 * 16;

        public static new Coroutine StartCoroutine(IEnumerator enumerator)
        {
            if (instance == null)
            {
                instance = new GameObject("MusicLoader").AddComponent<MusicLoadManager>();
                DontDestroyOnLoad(instance);
            }

            return ((MonoBehaviour)instance).StartCoroutine(enumerator);
        }
        
        /// <summary>
        /// Loads music from folder
        /// </summary>
        /// <param name="pathToFolder">Path to folder to load music (default: "Custom Songs", if player is host)</param>
        /// <param name="resetLoadedClips">Clears loaded clips</param>
        public static void StartLoadMusic(string pathToFolder = "Custom Songs", bool resetLoadedClips = true)
        {
            StartCoroutine(LoadMusic(pathToFolder, resetLoadedClips));
        }
        protected static IEnumerator LoadMusic(string pathToFolder = "Custom Songs", bool resetLoadedClips = true)
        {
            if (resetLoadedClips) AudioClips.Clear();
            string path = Path.Combine(Paths.GameRootPath,"Plugins/Boombox", pathToFolder);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            foreach (string file in Directory.GetFiles(path))
            {
                if (AudioClips.Keys.Any(t => t.StartsWith(file))) continue; 
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
                            clip.name = HashAudioClipName(Path.GetFileName(file), SHA256FromBytes(fileData).Substring(0, 16));
                            AudioClips.Add(clip.name,clip);

                            Boombox.log.LogInfo($"Music Loaded: {clip.name}");
                        }
                    }
                }
            }
            Boombox.log.LogInfo($"Finished loading all music!");
        }

        public static string DehashAudioClipName(string name) => name.Replace(_music_hash_splitter_replacer, "//");
        public static string HashAudioClipName(string name, string hash) => name.Replace("//",_music_hash_splitter_replacer) + "//" + hash;

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

        public static byte[] GetChunk(AudioClip clip, int i)
        {
            return ClipsToByte(clip).Skip(i * ChunkSize).Take(ChunkSize).ToArray();
        }

        public static byte[] GetChunk(string clip, int i) => GetChunk(AudioClips[clip], i);
            
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
