using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using FantomLis.BoomboxExtended.Settings;
using FantomLis.BoomboxExtended.Utils;
using HarmonyLib;
using MyceliumNetworking;
using UnityEngine;
using UnityEngine.Localization.Settings;
using Zorro.Core;
using Zorro.UI.Modal;

namespace FantomLis.BoomboxExtended
{
#if BepInEx
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)] // BepInEx compatibility 
#endif
    [ContentWarningPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_VERSION, false)]
#if BepInEx
    public class Boombox : BaseUnityPlugin
#else
    public class Boombox : MonoBehaviour
#endif
    {
        public const string ItemName = "Boombox";
        internal static Boombox Self;
        /// <summary>
        /// Client-only setting, enables verbose logging
        /// <remarks>Always returns true when BepInEx build</remarks>
        /// </summary>
        #if !BepInEx
        public static bool IsDebug => GameHandler.Instance?.SettingsHandler?.GetSetting<VerboseLoggingSetting>().Value ?? false;
        #else
        public static bool IsDebug => true;
        #endif
        /// <summary>
        /// Global setting, sets battery capacity for Boombox in the shop
        /// </summary>
        public static BatteryCapacitySetting BatteryCapacity;
        /// <summary>
        /// Current battery capacity for this lobby (not from config)
        /// </summary>
        public static float CurrentBatteryCapacity { protected set; get; }
        /// <summary>
        /// Current boombox price for this lobby (not from config)
        /// </summary>
        public static int CurrentBoomboxPrice{ protected set; get; }
        /// <summary>
        /// Current pausing music setting (not from config)
        /// </summary>
        public static bool CurrentPauseMusic{ protected set; get; }
        /// <summary>
        /// Client-only setting, selects how music selection works
        /// </summary>
        public static MusicSelectionMethodSetting BoomboxMethod;

        /// <summary>
        /// Client-only setting, sets volume change step
        /// </summary>
        public static VolumeStepSetting VolumeStep;
        
        /// <summary>
        /// Global setting, sets price for boombox in shop
        /// </summary>
        public static BoomboxPriceSetting BoomboxPrice;

        /// <summary>
        /// Global setting, sets pausing music instead of stopping
        /// </summary>
        public static PauseMusicSetting PauseMusicSetting;

        public static MusicSelectionMethodSetting.BoomboxMusicSelectionMethod CurrentBoomboxMethod() =>
            (MusicSelectionMethodSetting.BoomboxMusicSelectionMethod) BoomboxMethod.Value;
        
        internal static VolumeUpSetting VolumeUpKey;
        internal static VolumeDownSetting VolumeDownKey;

        const string _boomboxBCID = $"{ItemName}.BatteryCapacity";
        const string _boomboxBPID = $"{ItemName}.BoomboxPrice";
        const string _boomboxPMID = $"{ItemName}.PauseMusic";
        
        private static readonly Harmony Harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        internal static Item BoomboxItem;
#if BepInEx
        public static ManualLogSource Log;
#endif
        static Boombox()
        {
            Self = GameObjectUtils.MakeNewDontDestroyOnLoad($"{ItemName}Loader").AddComponent<Boombox>();
#if BepInEx
            Log = Self.Logger;
#endif      
            Self.Awake();
        }
        
        void Awake()
        {
            LogUtils.LogDebug("Pre-Loading started...");
            LogUtils.LogDebug("Patching started...");
            Harmony.PatchAll();
            LogUtils.LogDebug("Patching finished.");
            LogUtils.LogDebug("Pre-Loading finished.");
            LogUtils.LogInfo("Pre-game load finished!");
        }

        private void EventRegister()
        {
            MyceliumNetwork.Initialize();
            MyceliumNetwork.LobbyCreated += () =>
            {
                if (MyceliumNetwork.IsHost)
                {
                    MyceliumNetwork.SetLobbyData(_boomboxBCID,
                        BatteryCapacity.Value);
                    MyceliumNetwork.SetLobbyData(_boomboxBPID, BoomboxPrice.Value);
                    MyceliumNetwork.SetLobbyData(_boomboxPMID, PauseMusicSetting.Value);
                    MusicLoadManager.StartLoadMusic();
                }
            };
            MyceliumNetwork.LobbyEntered += () => x();  
            MyceliumNetwork.LobbyLeft += () =>
            {
                CurrentBatteryCapacity = BatteryCapacity.Value;
                CurrentBoomboxPrice = BoomboxPrice.Value;
                BoomboxItem.price = CurrentBoomboxPrice;
                CurrentPauseMusic = PauseMusicSetting.Value;
                MusicLoadManager.UnloadMusic();
            };
            MyceliumNetwork.LobbyDataUpdated += (a) => x();  
            LogUtils.LogDebug("All events registered.");

            void x()
            {
                if (!MyceliumNetwork.IsHost)
                {
                    CurrentBatteryCapacity = MyceliumNetwork.GetLobbyData<float>(_boomboxBCID);
                    CurrentBoomboxPrice = MyceliumNetwork.GetLobbyData<int>(_boomboxBPID);
                    CurrentPauseMusic = MyceliumNetwork.GetLobbyData<bool>(_boomboxPMID);
                    BoomboxItem.price = CurrentBoomboxPrice;
                }
            }
        }

        void Start()
        {
            FindUnmigratedMusic();
            LogUtils.LogDebug("Loading started...");
            EventRegister();
            LoadConfig();
            LoadBoombox();
            LogUtils.LogDebug("Loading finished.");
        }

        private static void FindUnmigratedMusic()
        {
            LogUtils.LogDebug("Searching for old music folders to migrate...");
            _migrate_folder(Path.Combine(Application.dataPath, "..", "BepInEx", "plugins", "Custom Songs"));
            _migrate_folder(Path.Combine(Application.dataPath, "..", "Plugins", "Custom Songs"));
            LogUtils.LogDebug("Searching done...");
            void _migrate_folder(string path)
            {
                if (Directory.Exists(path))
                {
                    LogUtils.LogInfo($"Found {path} music folder. Migrating...");
                    string filename;
                    bool isSomethingWasThere = false;
                    string[] files = Directory.GetFiles(path);
                    foreach (var _f_path in files)
                    {
                        isSomethingWasThere = true;
                        filename = Path.GetFileName(_f_path);
                        if (File.Exists(Path.Combine(MusicLoadManager.RootPath, MusicLoadManager.HostFolder, filename)))
                        {
                            LogUtils.LogWarning($"Found file ({filename}) with same name in destination folder. Skipping...");
                            continue;
                        }
                        if (!MusicLoadManager.SupportedFormats.Contains(Path.GetExtension(filename).TrimStart('.')))
                        {
                            LogUtils.LogWarning($"Unknown audio file {filename}, skipping...");
                            continue;
                        }
                        File.Move(_f_path, Path.Combine(MusicLoadManager.RootPath, MusicLoadManager.HostFolder, filename));
                        LogUtils.LogInfo($"Migrated {filename} to new folder.");
                    }

                    if (Directory.GetFiles(path).Length > 0 && Directory.GetDirectories(path).Length > 0)
                    {
                        LogUtils.LogWarning($"Not all files (or folders) was migrated to new folder. Please remove {path} manually.");
                    }
                    else if (isSomethingWasThere)
                    {
                        LogUtils.LogInfo($"Folder {path} migrated. Removing folder...");
                        Directory.Delete(path);
                    }
                    else
                    {
                        LogUtils.LogWarning($"Folder {path} empty, skipping");
                    }
                }
            }
        }
        
        private static void LoadConfig()
        {
            LogUtils.LogDebug($"Config loading started...");

            #region Load settings

            VolumeUpKey = GameHandler.Instance.SettingsHandler.GetSetting<VolumeUpSetting>();
            VolumeDownKey = GameHandler.Instance.SettingsHandler.GetSetting<VolumeDownSetting>();
            BatteryCapacity = GameHandler.Instance.SettingsHandler.GetSetting<BatteryCapacitySetting>();
            BoomboxMethod = GameHandler.Instance.SettingsHandler.GetSetting<MusicSelectionMethodSetting>();
            BoomboxPrice =  GameHandler.Instance.SettingsHandler.GetSetting<BoomboxPriceSetting>();
            CurrentBatteryCapacity = BatteryCapacity.Value;
            CurrentBoomboxPrice = BoomboxPrice.Value;
            VolumeStep = GameHandler.Instance.SettingsHandler.GetSetting<VolumeStepSetting>();
            PauseMusicSetting = GameHandler.Instance.SettingsHandler.GetSetting<PauseMusicSetting>();
            CurrentPauseMusic = PauseMusicSetting.Value;

            #endregion

            #region Registering Lobby Data

            MyceliumNetwork.RegisterLobbyDataKey(_boomboxBCID);
            MyceliumNetwork.RegisterLobbyDataKey(_boomboxBPID);
            MyceliumNetwork.RegisterLobbyDataKey(_boomboxPMID);

            #endregion
            
            LogUtils.LogDebug($"Boombox loaded with settings: Battery capacity: {BatteryCapacity.Value}, Music Selection method: {((MusicSelectionMethodSetting.BoomboxMusicSelectionMethod)BoomboxMethod.Value).ToString()}, Boombox Price {CurrentBoomboxPrice}");
        }

        private static void LoadBoombox()
        {
            try
            {
                string boomboxAssetbundle = "boombox.assetBundle";
                AssetBundle asset = QuickLoadAssetBundle(boomboxAssetbundle); // Why boombox not using .assetBundle filetype?
                BoomboxItem = asset.LoadAsset<Item>(ItemName);
                BoomboxItem.itemObject.AddComponent<BoomboxBehaviour>();
                BoomboxItem.price = GameHandler.Instance.SettingsHandler.GetSetting<BoomboxPriceSetting>().Value;

                LogUtils.LogDebug($"Resource {boomboxAssetbundle} loaded!");
                
                SingletonAsset<ItemDatabase>.Instance.AddRuntimeEntry(BoomboxItem);
                SingletonAsset<PropContentDatabase>.Instance.AddRuntimeEntry(BoomboxItem.content);
                LogUtils.LogDebug("Loading boombox finished!");
            }
            catch (Exception ex)
            {
                LogUtils.LogFatal($"Boombox failed to load: {ex.Message} \n({ex.StackTrace})");
            }
        }
        
        internal static void RegisterBoomboxAsArtifact()
        {
            if (RoundArtifactSpawner.me && !RoundArtifactSpawner.me.possibleSpawns.Contains(BoomboxItem))
                RoundArtifactSpawner.me.possibleSpawns =
                RoundArtifactSpawner.me.possibleSpawns.AddRangeToArray([BoomboxItem]);
        }
        
        internal static AssetBundle QuickLoadAssetBundle(string name)
        {
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, name);
            return AssetBundle.LoadFromFile(path);
        }
    }
}
