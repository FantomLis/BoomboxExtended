using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.IO;
using System.Linq;
using System.Reflection;
using FantomLis.BoomboxExtended.Settings;
using MyceliumNetworking;
using Unity.Mathematics;
/*using ShopUtils;*/
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using Zorro.Core;
using Zorro.Settings;

namespace FantomLis.BoomboxExtended
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [ContentWarningPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_VERSION, false)]
    //[BepInDependency("hyydsz-ShopUtils")] // Partially compatible with new version
    [BepInDependency("RugbugRedfern.MyceliumNetworking", BepInDependency.DependencyFlags.HardDependency)]
    public class Boombox : BaseUnityPlugin
    {
        public static ManualLogSource log;

        public static AssetBundle asset;
        public const string ItemName = "Boombox";
        
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
        public static readonly uint modId = 614702256;
        static string _boomboxBCID = $"{ItemName}.BatteryCapacity";
        static string _boomboxBPID = $"{ItemName}.BoomboxPrice";
        
        private static readonly Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

        private static Item BoomboxItem;

        static Boombox()
        {
            new GameObject($"{ItemName}Loader").AddComponent<Boombox>().Awake();
        }
        

        private static Dictionary<string, byte[]> _loadingFiles = new();
        [CustomRPC]
        public void ReceiveAudioClip(string name, byte[] data, bool isFinalChunk)
        {
            Boombox.log.LogInfo($"Loading music file {name}...");
            _loadingFiles[name] = _loadingFiles[name].Concat(data).ToArray();

            if (isFinalChunk)
            {
                string path = Path.Combine(Paths.PluginPath, MyceliumNetwork.LobbyHost.m_SteamID.ToString(), name);
                File.WriteAllBytes(path, _loadingFiles[name]);
            }
        }

        [CustomRPC]
        public void RequestAudioClips(RPCInfo info)
        {
            foreach (var clip in MusicLoadManager.AudioClips.Values)
            {
                MusicLoadManager.StartCoroutine(RequestAudioClip(clip, info));
            }
            MyceliumNetwork.RPCTarget(modId, nameof(AudioClipsLoaded), info.SenderSteamID, ReliableType.Reliable);
        }
        
        public IEnumerator RequestAudioClip(AudioClip c, RPCInfo info)
        {
            for (int x = 0; x <= MusicLoadManager.ClipsToByte(c).Length; x += MusicLoadManager.ChunkSize)
            {
                MyceliumNetwork.RPCTarget(modId, nameof(ReceiveAudioClip), info.SenderSteamID, ReliableType.Reliable, MusicLoadManager.GetChunk(c,x/256));
                yield return new WaitForSeconds(AudioSendChunkTime);
            }
        }

        private const float AudioSendChunkTime = 0.05f;

        [CustomRPC]
        public void AudioClipsLoaded( RPCInfo info)
        {
            if (info.SenderSteamID == MyceliumNetwork.LobbyHost)
                MusicLoadManager.StartLoadMusic(MyceliumNetwork.LobbyHost.m_SteamID.ToString(), true);
            else
            {
                log.LogWarning($"Player {info.SenderSteamID} is not a host, but sent AudioClipsLoaded RPC event!");
            }
        }
        
        void Awake()
        {
            log = Logger;
            log.LogDebug("Pre-Loading started...");
            MyceliumNetwork.RegisterNetworkObject(this, modId);
            log.LogDebug("Patching started...");
            harmony.PatchAll();
            log.LogDebug("Patching finished.");
            LoadLanguages();
            log.LogDebug("Pre-Loading finished.");
            log.LogInfo("Pre-game load finished!");
        }

        private void EventRegister()
        {
            MyceliumNetwork.LobbyLeft += () => _loadingFiles.Clear();
            MyceliumNetwork.LobbyEntered += () =>
            {
                MyceliumNetwork.RPCTarget(modId, nameof(RequestAudioClips), MyceliumNetwork.LobbyHost, ReliableType.Reliable);
            };
            MyceliumNetwork.LobbyCreated += () =>
            {
                MusicLoadManager.StartLoadMusic("Custom Songs", true);
            };
            MyceliumNetwork.LobbyCreated += () =>
            {
                if (MyceliumNetwork.IsHost)
                {
                    MyceliumNetwork.SetLobbyData(_boomboxBCID,
                        BatteryCapacity.Value);
                    MyceliumNetwork.SetLobbyData(_boomboxBPID, BoomboxPrice.Value);
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
            log.LogDebug("All events registered.");

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
            log.LogDebug("Loading started...");
            EventRegister();
            LoadConfig();
            LoadBoombox();
            log.LogDebug("Music loading started...");
            MusicLoadManager.StartLoadMusic();
            log.LogDebug("Music loading finished.");
            log.LogInfo("Music is ready!");
            log.LogDebug("Loading finished.");
        }

        private static void LoadConfig()
        {
            log.LogDebug($"Config loading started...");
            VolumeUpKey = GameHandler.Instance.SettingsHandler.GetSetting<VolumeUpSetting>();
            VolumeDownKey = GameHandler.Instance.SettingsHandler.GetSetting<VolumeDownSetting>();
            BatteryCapacity = GameHandler.Instance.SettingsHandler.GetSetting<BatteryCapacitySetting>();
            BoomboxMethod = GameHandler.Instance.SettingsHandler.GetSetting<MusicSelectionMethodSetting>();
            BoomboxPrice =  GameHandler.Instance.SettingsHandler.GetSetting<BoomboxPriceSetting>();
            CurrentBatteryCapacity = BatteryCapacity.Value;
            CurrentBoomboxPrice = BoomboxPrice.Value;
            
            MyceliumNetwork.RegisterLobbyDataKey(_boomboxBCID);
            MyceliumNetwork.RegisterLobbyDataKey(_boomboxBPID);
            
            log.LogDebug($"Boombox loaded with settings: Battery capacity: {BatteryCapacity.Value}, Music Selection method: {BoomboxMethod}, Boombox Price {CurrentBoomboxPrice}");
        }


        private static void LoadBoombox()
        {
            string boomboxAssetbundle = "boombox.assetBundle";
            try
            {
                asset = QuickLoadAssetBundle(boomboxAssetbundle); // Why boombox not using .assetBundle filetype?
                
                BoomboxItem = asset.LoadAsset<Item>(ItemName);
                BoomboxItem.itemObject.AddComponent<BoomboxBehaviour>();
                BoomboxItem.Category = ShopItemCategory.Misc;
                BoomboxItem.purchasable = true;
                BoomboxItem.price = GameHandler.Instance.SettingsHandler.GetSetting<BoomboxPriceSetting>().Value;

                log.LogDebug($"Resource {boomboxAssetbundle} loaded!");
                
                //Entries.RegisterAll();
                SingletonAsset<ItemDatabase>.Instance.AddRuntimeEntry(BoomboxItem);
                /*RoundArtifactSpawner.me.possibleSpawns =
                    RoundArtifactSpawner.me.possibleSpawns.Concat([BoomboxItem]).ToArray();*/
                log.LogDebug("Loading boombox finished!");
            }
            catch (Exception ex)
            {
                log.LogFatal($"Boombox failed to load: {ex.Message} ({ex.StackTrace})");
            }
        }

        private void LoadLanguages()
        {
            LocalizationStrings.Culture = CultureInfo.GetCultureInfo(LocalizationSettings.SelectedLocale.Identifier.CultureInfo.LCID);
        }
        
        public static AssetBundle QuickLoadAssetBundle(string name)
        {
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, name);
            return AssetBundle.LoadFromFile(path);
        }

        public void OnDestroy()
        {
            MyceliumNetwork.DeregisterNetworkObject(this, modId);
        }
    }

    public class BatteryCapacityConfig : FloatSetting, IExposedSetting
    {
        public override void ApplyValue()
        {
            Boombox.log.LogDebug($"Value {this.GetType().Name} changed to {Value}");
        }
        protected override float GetDefaultValue() => 250f;
        protected override float2 GetMinMaxValue() => new float2(-1, 10000);
        public SettingCategory GetSettingCategory() => SettingCategory.Mods;
        public string GetDisplayName() => "Battery capacity (-1 for infinite capacity)";
    }
}
