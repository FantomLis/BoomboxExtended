using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FantomLis.BoomboxExtended.Containers;
using FantomLis.BoomboxExtended.Locales;
using FantomLis.BoomboxExtended.Utils;
using MyceliumNetworking;
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

        #region Watcher
        static FileSystemWatcher _watcher;
        private static void _makeWatcher(string path)
        {
            _watcher = new FileSystemWatcher();
            _watcher.Path = path;
            _watcher.Changed += (sender, args) =>
            {
                Music.Remove(Path.GetFileNameWithoutExtension(args.Name));
                AlertUtils.AddMoneyCellAlert(BoomboxLocalization.FileChangedAlert, MoneyCellUI.MoneyCellType.Revenue, $"{Path.GetFileNameWithoutExtension(args.Name)}");
                LoadThisMusic(args.FullPath);
            };
            _watcher.Created += (sender, args) =>
            {
                AlertUtils.AddMoneyCellAlert(BoomboxLocalization.FileFoundAlert, MoneyCellUI.MoneyCellType.Revenue, $"{Path.GetFileNameWithoutExtension(args.Name)}");
                LoadThisMusic(args.FullPath);
            };
            _watcher.Deleted += (sender, args) =>
            {
                var rm = Music.Remove(Path.GetFileNameWithoutExtension(args.Name));
                if (rm) AlertUtils.AddMoneyCellAlert(BoomboxLocalization.MusicUnloadedAlert, MoneyCellUI.MoneyCellType.Revenue, $"{Path.GetFileNameWithoutExtension(args.Name)}");
            };
            _watcher.Error += (sender, args) =>
            {
                LogUtils.LogError($"Unable to continue to monitor folder {path}: {args.GetException().ToString()}");
                AlertUtils.AddMoneyCellAlert(BoomboxLocalization.MusicHotLoaderFailedAlert, MoneyCellUI.MoneyCellType.HospitalBill, "", true);
            };
            _watcher.EnableRaisingEvents = true;
        }

        #endregion
        
        /// <summary>
        /// Default Root Path from which music is searching
        /// </summary>
        internal static string RootPath = Path.Combine(Application.dataPath, "Mod Resources", "Boombox");

        internal static string HostFolder = "Custom Songs";
        private static Coroutine _StartCoroutine(IEnumerator enumerator)
        {
            Instance ??= GameObjectUtils.MakeNewDontDestroyOnLoad("MusicLoader").AddComponent<MusicLoadManager>();
            return ((MonoBehaviour)Instance).StartCoroutine(enumerator);
        }

        internal static void LoadHostMusic()
        {
            AlertUtils.DropQueuedMoneyCellAlert(BoomboxLocalization.MusicLoadedAlert);
            MyceliumNetwork.RPCTarget(Boombox.modID, nameof(_RequestHostMusicList), MyceliumNetwork.LobbyHost, ReliableType.Reliable);
        }

        [CustomRPC]
        private static void _RequestHostMusicList(RPCInfo info)
        {
            MyceliumNetwork.RPCTarget(Boombox.modID, nameof(_ReceiveHostMusicList), info.SenderSteamID, ReliableType.Reliable, [Music.Values.ToArray().Select(x => Path.GetFileName(x.FilePath))]);
        }
        
        [CustomRPC]
        private static void _ReceiveHostMusicList(RPCInfo info, string[] musicNames)
        {
            string path = System.IO.Path.Combine(RootPath, MyceliumNetwork.LobbyHost.m_SteamID.ToString());
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string[] files = Directory.GetFiles(path).Select(Path.GetFileName).ToArray();
            foreach (var m in musicNames)
            {
                string fullFilePath = Path.Combine(RootPath, MyceliumNetwork.LobbyHost.ToString(), m);
                if (files.Contains(m)) LoadThisMusic(fullFilePath);
                else
                {
                    _DownloadMusicFile(path);
                }
            }
        }

        private static void _DownloadMusicFile(string path)
        {
            PreLoadMusic(path);
        }

        /// <summary>
        /// Loads music from folder
        /// </summary>
        public static void StartLoadMusic(bool reloadAllSongs = true)
        {
            if (isLoading) return;  
            isLoading = true;
            string path = System.IO.Path.Combine(RootPath, HostFolder);
            if (reloadAllSongs) Music.Clear();
            LoadMusic(path);
        }
        private static void LoadMusic(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            AlertUtils.DropQueuedMoneyCellAlert(BoomboxLocalization.MusicLoadedAlert);
            if (MyceliumNetworking.MyceliumNetwork.IsHost) _makeWatcher(path);
            foreach (string file in Directory.GetFiles(path))
            {
                LoadThisMusic(file);
            }
            
            Task.Run(() =>
            {
                Task.WaitAll(Task.Run(() => { while (__awaiting_tasks.Count > 0) {Thread.Sleep(100); } }));
                LogUtils.LogInfo($"Loading music finished! ({Music.Count} loaded)");
                AlertUtils.AddMoneyCellAlert(BoomboxLocalization.MusicLoadedAlert, MoneyCellUI.MoneyCellType.MetaCoins,
                    string.Format(BoomboxLocalization.MusicLoadedAlertDesc, Music.Count.ToString()),
                    dropQueuedAlert: true);
                isLoading = false;
            });
        }

        private static void PreLoadMusic(string file)
        {
            string name = Path.GetFileNameWithoutExtension(file);
            if (Music.ContainsKey(name)) return;
            var music = new Music(file);
            Music.Add(name, music);
        }

        private static void LoadThisMusic(string file)
        {
            string name = Path.GetFileNameWithoutExtension(file);
            if (Music.ContainsKey(name)) return;
            var music = new Music(file);
            var m_load_task = music.LoadMusic();
            __awaiting_tasks.Add(m_load_task);
            Task.Run(async () =>
            {
                Music.Add(name, music);
                try
                {
                    if (await m_load_task is false)
                    {
                        LogUtils.LogWarning($"Failed to load file {file}: File is not a supported audio file.");
                        AlertUtils.AddMoneyCellAlert(BoomboxLocalization.UnsupportedAudioFileAlert,
                            MoneyCellUI.MoneyCellType.HospitalBill, name);
                    }
                    else if (!music.Clip)    
                    {
                        LogUtils.LogError($"Failed to load file {file}: Audio is invalid.");
                        AlertUtils.AddMoneyCellAlert(BoomboxLocalization.FileInvalidAlert,
                            MoneyCellUI.MoneyCellType.HospitalBill, name);
                    }
                    else
                    {
                        LogUtils.LogInfo($"Song Loaded: {name}");
                        AlertUtils.AddMoneyCellAlert(BoomboxLocalization.SingleSongLoadedAlert,
                            MoneyCellUI.MoneyCellType.Revenue, name);
                    }
                }
                catch (Exception ex)
                {
                    LogUtils.LogWarning($"Failed to load file {file}: ({ex.ToString()})");
                    AlertUtils.AddMoneyCellAlert(BoomboxLocalization.FileLoadFailAlert,
                        MoneyCellUI.MoneyCellType.HospitalBill, ex.Message);
                }
                __awaiting_tasks.Remove(m_load_task);
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

        public static readonly string[] SupportedFormats = ["mp3", "ogg", "wav"];
        public static void UnloadMusic()
        {
            _StartCoroutine(_unloadMusic());
        }

        private static IEnumerator _unloadMusic()
        {
            yield return new WaitUntil(() => !isLoading);
            Music.Clear();
            _watcher.EnableRaisingEvents = false;
            _watcher = null;
        }
    }
}
