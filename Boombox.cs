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
        /// Client-only setting, selects how music selection works
        /// </summary>
        public static MusicSelectionMethodSetting BoomboxMethod;

        public static MusicSelectionMethodSetting.BoomboxMusicSelectionMethod CurrentBoomboxMethod() =>
            (MusicSelectionMethodSetting.BoomboxMusicSelectionMethod) BoomboxMethod.Value;
        public static BoomboxPriceSetting BoomboxPrice;
        public static VolumeUpSetting VolumeUpKey;
        public static VolumeDownSetting VolumeDownKey;

        static string _boomboxBCID = $"{ItemName}.BatteryCapacity";
        static string _boomboxBPID = $"{ItemName}.BoomboxPrice";
        
        private static readonly Harmony Harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        private static Item BoomboxItem;
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
                    MusicLoadManager.StartLoadMusic();
                }
            };
            MyceliumNetwork.LobbyEntered += () => x();  
            MyceliumNetwork.LobbyLeft += () =>
            {
                CurrentBatteryCapacity = BatteryCapacity.Value;
                CurrentBoomboxPrice = BoomboxPrice.Value;
                BoomboxItem.price = CurrentBoomboxPrice;
            };
            MyceliumNetwork.LobbyDataUpdated += (a) => x();  
            LogUtils.LogDebug("All events registered.");

            void x()
            {
                if (!MyceliumNetwork.IsHost)
                {
                    CurrentBatteryCapacity = MyceliumNetwork.GetLobbyData<float>(_boomboxBCID);
                    CurrentBoomboxPrice = MyceliumNetwork.GetLobbyData<int>(_boomboxBPID);
                    BoomboxItem.price = CurrentBoomboxPrice;
                }
            }
        }

        void Start()
        {
            LogUtils.LogDebug("Loading started...");
            EventRegister();
            LoadConfig();
            LoadBoombox();
            LogUtils.LogDebug("Loading finished.");
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

            #endregion

            #region Registering Lobby Data

            MyceliumNetwork.RegisterLobbyDataKey(_boomboxBCID);
            MyceliumNetwork.RegisterLobbyDataKey(_boomboxBPID);

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
