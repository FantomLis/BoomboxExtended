using System;
using System.Collections;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using ShopUtils;
using ShopUtils.Language;
using ShopUtils.Network;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MyceliumNetworking;
using UnityEngine;
using UnityEngine.Localization;

namespace FantomLis.BoomboxExtended
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [ContentWarningPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_VERSION, false)]
    [BepInDependency("hyydsz-ShopUtils")]
    [BepInDependency("RugbugRedfern.MyceliumNetworking", BepInDependency.DependencyFlags.HardDependency)]
    public class Boombox : BaseUnityPlugin
    {
        public static ManualLogSource log;

        public const string ModGUID = "hyydsz-Boombox";
        public const string ModName = "Boombox";
        public const string ModVersion = "1.1.5";

        private readonly Harmony harmony = new Harmony(ModGUID);

        public static AssetBundle asset;

        public static bool InfiniteBattery = false;
        public static float BatteryCapacity = 250f;
        public static float AudioSendChunkTime = 0.05f;

        public static ConfigEntry<KeyCode> VolumeUpKey;
        public static ConfigEntry<KeyCode> VolumeDownKey;
        public static readonly uint modId = 614702256 + uint.Parse(MyPluginInfo.PLUGIN_VERSION.Split('.')[0]);


        private static Dictionary<string, byte[]> _loadingFiles = new();
        [CustomRPC]
        public static void ReceiveAudioClip(string name, byte[] data, bool isFinalChunk)
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
        public static void RequestAudioClips(RPCInfo info)
        {
            foreach (var clip in MusicManager.AudioClips.Values)
            {
                MusicManager.StartCoroutine(RequestAudioClip(clip, info));
            }
            MyceliumNetwork.RPCTarget(modId, nameof(AudioClipsLoaded), info.SenderSteamID, ReliableType.Reliable);
        }
        
        public static IEnumerator RequestAudioClip(AudioClip c, RPCInfo info)
        {
            for (int x = 0; x <= MusicManager.ClipsToByte(c).Length; x += 256)
            {
                MyceliumNetwork.RPCTarget(modId, nameof(ReceiveAudioClip), info.SenderSteamID, ReliableType.Reliable, MusicManager.GetChunk(c,x/256));
                yield return new WaitForSeconds(AudioSendChunkTime);
            }
        }

        [CustomRPC]
        public static void AudioClipsLoaded( RPCInfo info)
        {
            if (info.SenderSteamID == MyceliumNetwork.LobbyHost)
                MusicManager.StartLoadMusic(MyceliumNetwork.LobbyHost.m_SteamID.ToString(), true);
            else
            {
                log.LogWarning($"Player {info.SenderSteamID} is not a host, but sent AudioClipsLoaded RPC event!");
            }
        }
        
        void Awake()
        {
            MyceliumNetwork.RegisterNetworkObject(this, modId);
            MyceliumNetwork.LobbyLeft += () => _loadingFiles.Clear();
            MyceliumNetwork.LobbyEntered += () =>
            {
                MyceliumNetwork.RPCTarget(modId, nameof(RequestAudioClips), MyceliumNetwork.LobbyHost, ReliableType.Reliable);
            };
            MyceliumNetwork.LobbyCreated += () =>
            {
                MusicManager.StartLoadMusic("Custom Song", true);
            };
            LoadConfig();
            LoadBoombox();
            LoadLangauge();

            harmony.PatchAll();
        }

        void Start()
        {
            MusicManager.StartLoadMusic();
        }

        private void LoadConfig()
        {
            VolumeUpKey = Config.Bind("Config", "VolumeUp", KeyCode.Equals);
            VolumeDownKey = Config.Bind("Config", "VolumeDown", KeyCode.Minus);

            log = Logger;

            Networks.SetNetworkSync(new Dictionary<string, object>
            {
                {"BoomboxInfiniteBattery", Config.Bind("Config", "InfiniteBattery", false).Value},
                {"BoomboxBattery", Config.Bind("Config", "BatteryCapacity", 250f).Value }
            },
            (dic) =>
            {
                // throw error
                InfiniteBattery = bool.Parse(dic["BoomboxInfiniteBattery"]);
                BatteryCapacity = float.Parse(dic["BoomboxBattery"]);

                Logger.LogInfo($"Boombox Load [InfiniteBattery: {InfiniteBattery}, BatteryCapacity: {BatteryCapacity}]");
            });
        }

        private void LoadBoombox()
        {
            asset = QuickLoadAssetBundle("boombox.assetBundle"); // Why boombox not using .assetBundle filetype?

            Item item = asset.LoadAsset<Item>("Boombox");
            item.itemObject.AddComponent<BoomboxBehaviour>();

            Entries.RegisterAll();
            Items.RegisterShopItem(item, ShopItemCategory.Misc, Config.Bind("Config", "BoomboxPrice", 100).Value);
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
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), name);
            return AssetBundle.LoadFromFile(path);
        }
    }
}
