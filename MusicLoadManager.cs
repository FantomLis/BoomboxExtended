using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FantomLis.BoomboxExtended.Containers;
using FantomLis.BoomboxExtended.Locales;
using FantomLis.BoomboxExtended.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace FantomLis.BoomboxExtended
{
    public class MusicLoadManager : MonoBehaviour
    {
        private static MusicLoadManager? Instance;
        public static Dictionary<string, Music> Music = new ();
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
            if (reloadAllSongs) Music.Clear();
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
            Music.Clear();
            AlertUtils.DropQueuedMoneyCellAlert(BoomboxLocalization.MusicLoadedAlert);
            foreach (string file in Directory.GetFiles(path))
            {
                string name = Path.GetFileNameWithoutExtension(file);
                if (Music.ContainsKey(name)) continue;
                var m_load = new Music(file);
                var m_load_task = m_load.LoadMusic();
                __awaiting_tasks.Add(m_load_task);
                Task.Run(async () =>
                {
                    await m_load_task;
                    Music.Add(name, m_load);
                    LogUtils.LogInfo($"Song Loaded: {name}");
                    AlertUtils.AddMoneyCellAlert(BoomboxLocalization.SingleSongLoadedAlert,
                        MoneyCellUI.MoneyCellType.Revenue, name);
                    __awaiting_tasks.Remove(m_load_task);
                });
            }
            
            Task.Run(() =>
            {
                Task.WaitAll(__awaiting_tasks.ToArray());
                LogUtils.LogInfo($"Loading music finished! ({Music.Count} loaded)");
                AlertUtils.AddMoneyCellAlert(BoomboxLocalization.MusicLoadedAlert, MoneyCellUI.MoneyCellType.MetaCoins,
                    string.Format(BoomboxLocalization.MusicLoadedAlertDesc, Music.Count.ToString()),
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
