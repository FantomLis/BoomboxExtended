using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.IO;
using System.Reflection;
using MyceliumNetworking;
using ShopUtils;
using ShopUtils.Language;
using ShopUtils.Network;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Serialization;

namespace FantomLis.BoomboxExtended
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [ContentWarningPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_VERSION, false)]
    [BepInDependency("hyydsz-ShopUtils")] // Not compatible with new version
    [BepInDependency("RugbugRedfern.MyceliumNetworking", BepInDependency.DependencyFlags.HardDependency)]
    public class Boombox : BaseUnityPlugin
    {
        public enum BoomboxMusicSelectionMethod : byte
        {
            SelectionUI = 0,
            ScrollWheel = 1,
            Original = 2,
            Default = 0
        }
        
        public static ManualLogSource log;

        public static AssetBundle asset;

        /// <summary>
        /// Global setting, sets should Boombox use battery or not.
        /// Deprecated, do not use.
        /// </summary>
        [Obsolete("Use BatteryCapacity == -1 instead", true)]
        public static bool InfiniteBattery = false;
        
        /// <summary>
        /// Global setting, sets battery capacity for Boombox in the shop
        /// </summary>
        public static float BatteryCapacity;
        /// <summary>
        /// Client-only setting, selects how music selection works
        /// </summary>
        public BoomboxMusicSelectionMethod BoomboxMethod = BoomboxMusicSelectionMethod.Default;

        public static ConfigEntry<KeyCode> VolumeUpKey;
        public static ConfigEntry<KeyCode> VolumeDownKey;
        
        private const string _Section = "Config";
        private ConfigEntry<float> _BatteryCapacityKey;
        private const string _BoomboxPrice = "BoomboxPrice";
        
        
        private readonly Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

        void Awake()
        {
            EventRegister();
            LoadConfig();
            LoadBoombox();
            LoadLangauge();

            harmony.PatchAll();
        }

        private void EventRegister()
        {
            MyceliumNetwork.LobbyCreated += () =>
            {
                MyceliumNetwork.SetLobbyData("Boombox.BatteryCapacity",
                    _BatteryCapacityKey.Value);
            };
            MyceliumNetwork.LobbyEntered += () =>
            {
                BatteryCapacity = MyceliumNetwork.GetLobbyData<float>("Boombox.BatteryCapacity");
            };
            MyceliumNetwork.LobbyLeft += () =>
            {
                BatteryCapacity = _BatteryCapacityKey.Value;
            };
        }

        void Start()
        {
            MusicLoadManager.StartLoadMusic();
        }

        private void LoadConfig()
        {
            VolumeUpKey = Config.Bind(_Section, "VolumeUp", KeyCode.Equals);
            VolumeDownKey = Config.Bind(_Section, "VolumeDown", KeyCode.Minus);
            _BatteryCapacityKey = Config.Bind(_Section, "BatteryCapacity", 250f,
                "Sets maximum battery capacity in seconds for boombox (-1 - infinite)");
            BoomboxMethod = Config.Bind(_Section, "BoomboxMusicSelectionType", BoomboxMusicSelectionMethod.Default, 
                "Sets how music selection works.").Value;
            log = Logger;

            MyceliumNetwork.RegisterLobbyDataKey("Boombox.BatteryCapacity");
            BatteryCapacity = _BatteryCapacityKey.Value;
            Debug.Log($"Boombox loaded with settings: Battery capacity: {BatteryCapacity}, Music Selection method: {BoomboxMethod}");
        }

        private void LoadBoombox()
        {
            asset = QuickLoadAssetBundle("boombox.assetBundle"); // Why boombox not using .assetBundle filetype?

            Item item = asset.LoadAsset<Item>("Boombox");
            item.itemObject.AddComponent<BoomboxBehaviour>();

            Entries.RegisterAll();
            Items.RegisterShopItem(item, ShopItemCategory.Misc, Config.Bind(_Section, _BoomboxPrice, 100, "Price for boombox.").Value);
            Networks.RegisterItemPrice(item);
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
