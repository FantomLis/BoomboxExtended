using System;
using System.Collections;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.IO;
using System.Reflection;
using FantomLis.BoomboxExtended.Settings;
using MyceliumNetworking;
using ShopUtils;
using ShopUtils.Language;
using ShopUtils.Network;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Serialization;

// TODO: Port this mod to SteamWorkshop 
namespace FantomLis.BoomboxExtended
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [ContentWarningPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_VERSION, false)]
    [BepInDependency("hyydsz-ShopUtils")] // Partially compatible with new version
    [BepInDependency("RugbugRedfern.MyceliumNetworking", BepInDependency.DependencyFlags.HardDependency)]
    public class Boombox : BaseUnityPlugin
    {
        public static ManualLogSource log;

        public static AssetBundle asset;
        
        /// <summary>
        /// Global setting, sets battery capacity for Boombox in the shop
        /// </summary>
        public static BatteryCapacitySetting BatteryCapacity;
        /// <summary>
        /// Current battery capacity for this lobby (not from config)
        /// </summary>
        public static float CurrentBatteryCapacity { protected set; get; }
        /// <summary>
        /// Client-only setting, selects how music selection works
        /// </summary>
        public static MusicSelectionMethodSetting BoomboxMethod;

        public static VolumeUpSetting VolumeUpKey;
        public static VolumeDownSetting VolumeDownKey;

        private static readonly string _Section = "Config";
        private static readonly string _BoomboxPrice = "BoomboxPrice";
        
        private static readonly Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

        static Boombox()
        {
            new GameObject("BoomboxLoader").AddComponent<Boombox>().Awake();
        }
        
        void Awake()
        {
            log = Logger;
            log.LogDebug("Pre-Loading started...");
            harmony.PatchAll();
            EventRegister();
            LoadLangauge();
            log.LogDebug("Pre-Loading finished.");
            log.LogDebug("Patching started...");
            log.LogDebug("Patching finished.");
            log.LogInfo("Pre-game load finished!");
        }

        private void EventRegister()
        {
            MyceliumNetwork.LobbyCreated += () =>
            {
                if (MyceliumNetwork.IsHost)
                {
                    MyceliumNetwork.RegisterLobbyDataKey("Boombox.BatteryCapacity");
                    MyceliumNetwork.SetLobbyData("Boombox.BatteryCapacity",
                        BatteryCapacity.Value);
                }
            };
            MyceliumNetwork.LobbyEntered += () =>
            {
                if (!MyceliumNetwork.IsHost) CurrentBatteryCapacity = MyceliumNetwork.GetLobbyData<float>("Boombox.BatteryCapacity");
            };  
            MyceliumNetwork.LobbyLeft += () =>
            {
                CurrentBatteryCapacity = BatteryCapacity?.Value ?? BatteryCapacitySetting.DefaultValue();
            };
            log.LogDebug("Event registered.");
        }

        void Start()
        {
            log.LogDebug("Loading started...");
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
            CurrentBatteryCapacity = BatteryCapacity.Value;
            
            log.LogDebug($"Boombox loaded with settings: Battery capacity: {BatteryCapacity.Value}, Music Selection method: {BoomboxMethod}");
        }

        private static void LoadBoombox()
        {
            string boomboxAssetbundle = "boombox.assetBundle";
            try
            {
                asset = QuickLoadAssetBundle(boomboxAssetbundle); // Why boombox not using .assetBundle filetype?

                Item item = asset.LoadAsset<Item>("Boombox");
                item.itemObject.AddComponent<BoomboxBehaviour>();

                log.LogDebug($"Resource {boomboxAssetbundle} loaded!");
                
                Entries.RegisterAll();
                Items.RegisterShopItem(item, ShopItemCategory.Misc, GameHandler.Instance.SettingsHandler.GetSetting<BoomboxPriceSetting>().Value);
                Networks.RegisterItemPrice(item);
                log.LogDebug("Loading boombox finished!");
            }
            catch (Exception ex)
            {
                log.LogFatal($"Boombox failed to load: {ex.Message} ({ex.StackTrace})");
            }
        }

        private void LoadLangauge()
        {
            /*Locale Chinese = Languages.GetLanguage(LanguageEnum.ChineseSimplified);
            Chinese.AddLanguage("Boombox_ToolTips", "[LMB] 播放;[RMB] 切换音乐");
            Chinese.AddLanguage("Boombox", "音响");
            Chinese.AddLanguage("BoomboxVolume", "{0}% 音量");

            Locale ChineseTraditional = Languages.GetLanguage(LanguageEnum.ChineseTraditional);
            ChineseTraditional.AddLanguage("Boombox_ToolTips", "[LMB] 播放;[RMB] 切換音樂");
            ChineseTraditional.AddLanguage("Boombox", "音響");
            ChineseTraditional.AddLanguage("BoomboxVolume", "{0}% 音量");

            Locale English = Languages.GetLanguage(LanguageEnum.English);
            English.AddLanguage("Boombox_ToolTips", "[LMB] Player;[RMB] Switch Music");
            English.AddLanguage("Boombox", "Boombox");
            English.AddLanguage("BoomboxVolume", "{0}% Volume");

            Locale French = Languages.GetLanguage(LanguageEnum.French);
            French.AddLanguage("Boombox_ToolTips", "[LMB] Jouer de la musique;[RMB] Changer de musique");
            French.AddLanguage("Boombox", "Audio portable");
            French.AddLanguage("BoomboxVolume", "{0}% Volume");

            Locale German = Languages.GetLanguage(LanguageEnum.German);
            German.AddLanguage("Boombox_ToolTips", "[LMB] Musik abspielen;[RMB] Musik wechseln");
            German.AddLanguage("Boombox", "Boom Box");
            German.AddLanguage("BoomboxVolume", "{0}% Volume");

            Locale Italian = Languages.GetLanguage(LanguageEnum.Italian);
            Italian.AddLanguage("Boombox_ToolTips", "[LMB] Riproduci musica;[RMB] Cambia musica");
            Italian.AddLanguage("Boombox", "boom box");
            Italian.AddLanguage("BoomboxVolume", "{0}% volume");

            Locale Japanese = Languages.GetLanguage(LanguageEnum.Japanese);
            Japanese.AddLanguage("Boombox_ToolTips", "[LMB] 音楽を流す;[RMB] 音楽を切り替える");
            Japanese.AddLanguage("Boombox", "ポータブルオーディオ");
            Japanese.AddLanguage("BoomboxVolume", "{0}% 音量");

            Locale Portuguese = Languages.GetLanguage(LanguageEnum.Portuguese);
            Portuguese.AddLanguage("Boombox_ToolTips", "[LMB] Tocar música;[RMB] Mudar a Música");
            Portuguese.AddLanguage("Boombox", "Sistema de áudio portátil");
            Portuguese.AddLanguage("BoomboxVolume", "{0}% volume");

            Locale Russian = Languages.GetLanguage(LanguageEnum.Russian);
            Russian.AddLanguage("Boombox_ToolTips", "[LMB] Музыка;[RMB] Переключить музыку");
            Russian.AddLanguage("Boombox", "Портативный звук");
            Russian.AddLanguage("BoomboxVolume", "{0}% Громкость");

            Locale Spanish = Languages.GetLanguage(LanguageEnum.Spanish);
            Spanish.AddLanguage("Boombox_ToolTips", "[LMB] Reproducir música;[RMB] Cambiar música");
            Spanish.AddLanguage("Boombox", "Traductor portátil");
            Spanish.AddLanguage("BoomboxVolume", "{0}% Volumen");

            Locale Ukrainian = Languages.GetLanguage(LanguageEnum.Ukrainian);
            Ukrainian.AddLanguage("Boombox_ToolTips", "[LMB] Грати музику;[RMB] Перемкнути музику");
            Ukrainian.AddLanguage("Boombox", "Портувана аудіосистема");
            Ukrainian.AddLanguage("BoomboxVolume", "{0}% гучність");

            Locale Korean = Languages.GetLanguage(LanguageEnum.Korean);
            Korean.AddLanguage("Boombox_ToolTips", "[LMB] 음악 재생;[RMB] 음악 전환");
            Korean.AddLanguage("Boombox", "휴대용 오디오");
            Korean.AddLanguage("BoomboxVolume", "{0}% 볼륨");

            Locale Swedish = Languages.GetLanguage(LanguageEnum.Swedish);
            Swedish.AddLanguage("Boombox_ToolTips", "[LMB] Spela musik;[RMB] Byt musik");
            Swedish.AddLanguage("Boombox", "Bärbart ljudsystem");
            Swedish.AddLanguage("BoomboxVolume", "{0}% volym");*/
        }

        public static AssetBundle QuickLoadAssetBundle(string name)
        {
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, name);
            return AssetBundle.LoadFromFile(path);
        }
    }
}
