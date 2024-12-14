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
using System.Text;
using FantomLis.BoomboxExtended.Settings;
using MyceliumNetworking;
/*using ShopUtils;*/
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Serialization;
using Zorro.Core;

// TODO: Port this mod to SteamWorkshop 
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

        protected static Dictionary<string, List<string>> AlertQueue = new();
        public static Boombox Self;
        
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
        
        private static readonly Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

        private static Item BoomboxItem;

        static Boombox()
        {
            Self = new GameObject($"{ItemName}Loader").AddComponent<Boombox>();
            DontDestroyOnLoad(Self.transform);
            Self.Awake();
        }
        
        void Awake()
        {
            log = Logger;
            log.LogDebug("Pre-Loading started...");
            log.LogDebug("Patching started...");
            harmony.PatchAll();
            log.LogDebug("Patching finished.");
            LoadLanguages();
            log.LogDebug("Pre-Loading finished.");
            log.LogInfo("Pre-game load finished!");
        }

        private void EventRegister()
        {
            MyceliumNetwork.LobbyCreated += () =>
            {
                if (MyceliumNetwork.IsHost)
                {
                    MyceliumNetwork.SetLobbyData(_boomboxBCID,
                        BatteryCapacity.Value);
                    MyceliumNetwork.SetLobbyData(_boomboxBPID, BoomboxPrice.Value);
                }
                MusicLoadManager.StartLoadMusic();
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
            Self.StartCoroutine(DrawAllPendingAlerts());
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

        public static void ShowRevenueAlert(string header, string body, bool forceNow = false)
        {
            if (forceNow)
            {
                ShowRevenueAlert(new KeyValuePair<string, List<string>>(header, [body]));
                return;
            }
            if (AlertQueue.TryGetValue(header, out List<string> list))
            {
                list.Add(body);
            }
            else AlertQueue.Add(header, new List<string>([body]));
        }

        private static IEnumerator DrawAllPendingAlerts()
        {
            while (Self)
            {
                yield return new WaitForSeconds(0.25f);
                if (MyceliumNetwork.InLobby) yield break;
                foreach (var v in AlertQueue)
                {
                    ShowRevenueAlert(v);
                }
            }
        }

        private static void ShowRevenueAlert(KeyValuePair<string, List<string>> v)
        {
            StringBuilder b = new();
            for (int i = 0; i < v.Value.Count; i++)
            {
                if (i >= 2)
                {
                    b.Append($"and more ({v.Value.Count - i})");
                    break;
                }

                b.Append(v.Value[i] + "\n");
            }
            UserInterface.ShowMoneyNotification(v.Key, b.ToString(),
                MoneyCellUI.MoneyCellType.Revenue);
        }
    }
}
