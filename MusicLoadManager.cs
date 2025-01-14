using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FantomLis.BoomboxExtended.Locales;
using FantomLis.BoomboxExtended.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace FantomLis.BoomboxExtended
{
    public class MusicLoadManager : MonoBehaviour
    {
        private static MusicLoadManager? Instance;
        public static Dictionary<string, AudioClip> clips = new ();
        private static bool isLoading = false;
        private static List<Task> __awaiting_tasks = new();
        /// <summary>
        /// Default Root Path from which music is searching
        /// </summary>
        internal static string RootPath = Path.Combine(Application.dataPath, "Mod Resources", "Boombox");
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
        private static IEnumerator LoadMusic(string path)
        {
            if (isLoading) yield break;
            isLoading = true;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            clips.Clear();
            AlertUtils.DropQueuedMoneyCellAlert(BoomboxLocalization.MusicLoadedAlert);
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
                    
                    yield return new WaitUntil(() => handler.isDone);
                    if (loader.error == null)
                    {
                        try
                        {
                            Task? finishingLoadingTask = null;
                            finishingLoadingTask = Task.Run(() =>
                            {
                                AudioClip clip = DownloadHandlerAudioClip.GetContent(loader);
                                if (clip && clip.loadState == AudioDataLoadState.Loaded && clip.length != 0)
                                {
                                    clip.name = Path.GetFileNameWithoutExtension(file);
                                    if (!clips.TryAdd(clip.name, clip)) return; 

                                    LogUtils.LogInfo($"Song Loaded: {clip.name}");
                                    AlertUtils.AddMoneyCellAlert(BoomboxLocalization.SingleSongLoadedAlert,
                                        MoneyCellUI.MoneyCellType.Revenue, clip.name);
                                    __awaiting_tasks.Remove(finishingLoadingTask);
                                }
                            });
                            __awaiting_tasks.Add(finishingLoadingTask);
                        }
                        catch (Exception ex)
                        {
                            LogUtils.LogError(ex);
                        }
                    }
                    else
                    {
                        LogUtils.LogWarning($"Failed to load file {file}: ({loader.error})");
                    }
                }
            }

            Task.Run(() =>
            {
                Task.WaitAll(__awaiting_tasks.ToArray());
                LogUtils.LogInfo($"Loading music finished! ({clips.Count} loaded)");
                AlertUtils.AddMoneyCellAlert(BoomboxLocalization.MusicLoadedAlert, MoneyCellUI.MoneyCellType.MetaCoins,
                    string.Format(BoomboxLocalization.MusicLoadedAlertDesc, clips.Count.ToString()),
                    dropQueuedAlert: true);
                isLoading = false;
            });
        }

        public static AudioType GetAudioType(string path)
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
