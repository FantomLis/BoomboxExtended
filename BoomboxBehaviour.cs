using System;
using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FantomLis.BoomboxExtended.Settings;
using FantomLis.BoomboxExtended.Utils;
using Sirenix.Utilities;
using UnityEngine;
using Zorro.ControllerSupport;
using Zorro.Core.Serizalization;
namespace FantomLis.BoomboxExtended
{
    public class BoomboxBehaviour : ItemInstanceBehaviour
    {
        private static readonly float uiSize = 0.4f;
        private static readonly float SongButtonSize = 25f;

        private BatteryEntry batteryEntry;
        private OnOffEntry onOffEntry;
        private TimeEntry timeEntry;
        private VolumeEntry volumeEntry;
        private MusicEntry musicEntry;

        private SFX_PlayOneShot Click;
        private AudioSource Music;
        private float lastChangeTime;
        private bool openUI;
        private Vector2 selectionScroll = new Vector2();
        Rect windowRect = new ((Screen.width - Screen.width * uiSize)/2f , (Screen.height - Screen.height * uiSize)/2f , Screen.width*uiSize, Screen.height*uiSize);
        private void OnGUI()
        {
            if (openUI)
            {
                GUI.BeginGroup(windowRect);
                float h = MusicLoadManager.clips.Count * SongButtonSize * Screen.height / 1080f;
                selectionScroll = GUI.BeginScrollView(new Rect(0,0, windowRect.width-40, windowRect.height-40), selectionScroll, 
                    new Rect(0,0,windowRect.width-80,h));
                var x = GUI.SelectionGrid(new Rect(0,0,windowRect.width-40, h), musicEntry.MusicIndex, MusicLoadManager.clips.Keys.ToArray(), 1);
                if (x != musicEntry.MusicIndex) if (!TryUpdateMusic(x)) LogUtils.LogError($"Failed to change music to index {x}.");
                GUI.EndScrollView();
                GUI.EndGroup();
            }
        }

        private bool TryUpdateMusic(int index)
        {
            if (musicEntry.TryUpdateMusicEntry(index))
            {
                _updateMusic();
                return true;
            }
            return false;
        }
        
        private bool TryUpdateMusic(string id)
        {
            if (musicEntry.TryUpdateMusicEntry(id))
            {
                _updateMusic();
                return true;
            }
            return false;
        }
        
        private void NextMusic()
        {
            musicEntry.NextMusic();
            _updateMusic();
        }
        
        private void PreviousMusic()
        {
            musicEntry.PreviousMusic();
            _updateMusic();
        }

        private void _updateMusic()
        {
            timeEntry.currentTime = 0;
            timeEntry.SetDirty();
            Click.Play();
            lastChangeTime = Time.time;
            musicEntry.UpdateMusicName();
            Music.clip = MusicLoadManager.clips[musicEntry.MusicID];
            Music.time = timeEntry.currentTime;
        }

        void Awake()
        {
            Click = transform.Find("SFX/Click").GetComponent<SFX_PlayOneShot>();
            Music = GetComponent<AudioSource>();
            Music.outputAudioMixerGroup = GameHandler.Instance?.SettingsHandler.GetSetting<SFXVolumeSetting>()?.mixerGroup ?? Music.outputAudioMixerGroup;
        }

        public override void ConfigItem(ItemInstanceData data, PhotonView playerView)
        {
            if (Boombox.BatteryCapacity.Value >= 0)
            {
                if (!data.TryGetEntry(out batteryEntry))
                {
                    batteryEntry = new BatteryEntry()
                    {
                        m_charge = Boombox.BatteryCapacity.Value,
                        m_maxCharge = Boombox.BatteryCapacity.Value
                    };

                    data.AddDataEntry(batteryEntry);
                }
            }

            if (!data.TryGetEntry(out onOffEntry))
            {
                onOffEntry = new OnOffEntry()
                {
                    on = false
                };

                data.AddDataEntry(onOffEntry);
            }

            if (!data.TryGetEntry(out timeEntry))
            {
                timeEntry = new TimeEntry()
                {
                    currentTime = 0f
                };

                data.AddDataEntry(timeEntry);
            }

            if (!data.TryGetEntry(out musicEntry))
            {
                musicEntry = new MusicEntry();
                data.AddDataEntry(musicEntry);
            }

            if (!data.TryGetEntry(out volumeEntry))
            {
                volumeEntry = new VolumeEntry(50);

                data.AddDataEntry(volumeEntry);
            }
            
        }

        void Update()
        {
            if (isHeldByMe)
            {
                musicEntry.InitializeEntry();

                switch (Boombox.CurrentBoomboxMethod())
                {
                    case MusicSelectionMethodSetting.BoomboxMusicSelectionMethod.Original:
                    default:
                        if (Player.localPlayer.HasLockedInput()) break;
                        if (Player.localPlayer.input.aimWasPressed)
                        {
                            if (MusicLoadManager.clips.Count <= 0)
                            {
                                HelmetText.Instance.SetHelmetText("No Music", 2f); // TODO: To Alert utils
                                break;
                            }
                            NextMusic();
                        }
                        break;
                    case MusicSelectionMethodSetting.BoomboxMusicSelectionMethod.SelectionUIMouse:
                        openUI = (Input.GetKey(KeyCode.Mouse1) || Player.localPlayer.input.aimIsPressed) &&
                                 MusicLoadManager.clips.Count > 0;
                        Player.localPlayer.data.isInTitleCardTerminal = openUI;
                        if (openUI)
                        {
                            if (Input.GetKeyDown(KeyCode.Mouse1)) Click.Play();
                            if (MusicLoadManager.clips.Count <= 0)
                            {
                                HelmetText.Instance.SetHelmetText("No Music", 2f); // TODO: To Alert utils
                            }
                        }
                        break;
                    case MusicSelectionMethodSetting.BoomboxMusicSelectionMethod.SelectionUIScroll:
                        case MusicSelectionMethodSetting.BoomboxMusicSelectionMethod.Default:
                        openUI = (Input.GetKey(KeyCode.Mouse1) || Player.localPlayer.input.aimIsPressed) &&
                                 MusicLoadManager.clips.Count > 0;
                        if (Player.localPlayer.HasLockedInput()) break;
                        if (Input.GetAxis("Mouse ScrollWheel") * 10 * -1 != 0  && lastChangeTime + 0.1f <= Time.time && openUI)
                        {
                            if (Player.localPlayer.input.aimWasPressed) Click.Play();
                            if (MusicLoadManager.clips.Count <= 0)
                            {
                                HelmetText.Instance.SetHelmetText("No Music", 2f); // TODO: To Alert utils
                                break;
                            }
                            if (Input.GetAxis("Mouse ScrollWheel") * 10 * -1 >= 1) NextMusic();
                            else if (Input.GetAxis("Mouse ScrollWheel") * 10 * -1 <= -1) PreviousMusic();
                        }
                        selectionScroll = Vector2.up * ((musicEntry.MusicIndex * SongButtonSize * (Screen.height / 1080f))-(MusicLoadManager.clips.Count * SongButtonSize * Screen.height/1080f/2f));
                        break;
                    case MusicSelectionMethodSetting.BoomboxMusicSelectionMethod.ScrollWheel:
                        if (Player.localPlayer.HasLockedInput()) break;
                        if (Input.GetAxis("Mouse ScrollWheel") * 10 * -1 != 0  && lastChangeTime + 0.1f <= Time.time)
                        {
                            if (Player.localPlayer.input.aimWasPressed) Click.Play();
                            if (MusicLoadManager.clips.Count <= 0)
                            {
                                HelmetText.Instance.SetHelmetText("No Music", 2f); // TODO: To Alert utils
                                break;
                            }
                            if (Input.GetAxis("Mouse ScrollWheel") * 10 * -1 >= 1) NextMusic();
                            else if (Input.GetAxis("Mouse ScrollWheel") * 10 * -1 <= -1) PreviousMusic();
                        }
                        break;
                }
                if (!Player.localPlayer.HasLockedInput())
                {
                    if (Player.localPlayer.input.clickWasPressed)
                    {
                        if (MusicLoadManager.clips.Count == 0) 
                        {
                            HelmetText.Instance.SetHelmetText("No Music", 2f);
                        }
                        else
                        {
                            onOffEntry.on = !onOffEntry.on;
                            onOffEntry.SetDirty();
                        }

                        Click.Play();
                    }

                    if (GlobalInputHandler.GetKeyUp(Boombox.VolumeUpKey.Keycode()))
                    {
                        volumeEntry.PlusVolume(10); // TODO: To config parameter

                        Click.Play();
                    }
                    if (GlobalInputHandler.GetKeyUp(Boombox.VolumeDownKey.Keycode()))
                    {
                        volumeEntry.MinusVolume(10); // TODO: To config parameter
                        Click.Play();
                    }
                }
            }
            if (Boombox.BatteryCapacity.Value >= 0 && batteryEntry.m_charge < 0f) {
                onOffEntry.on = false;
            }

            #region Sync on/off state

            bool flag = onOffEntry.on;
            if (flag != Music.isPlaying)
            {
                if (flag && MusicLoadManager.clips.TryGetValue(musicEntry.MusicID, out var clip))
                {
                    Music.clip = clip;
                    Music.time = timeEntry.currentTime;
                    Music.Play();
                }
                else
                {
                    Music.Stop();
                }
            }

            #endregion

            #region Update boombox

            batteryEntry.m_charge -= (flag && Boombox.BatteryCapacity.Value >= 0 && batteryEntry.m_charge >= 0f ? Time.deltaTime : 0);
            timeEntry.currentTime = Music.time;
            Music.volume = volumeEntry.GetVolume();

            #endregion
        }
    }

    public class VolumeEntry : ItemDataEntry, IHaveUIData
    {
        public int Volume { get; private set; }

        private string VolumeText;

        /// <summary>
        /// Updates volume
        /// </summary>
        /// <param name="vol">Volume from 0 to 100</param>
        public void UpdateVolume(int vol)
        {
            Volume = Math.Clamp(vol, 0, 100);
            SetDirty();
        }

        public void MinusVolume(int vol) => UpdateVolume(Volume - vol);
        public void PlusVolume(int vol) => UpdateVolume(Volume + vol);

        public VolumeEntry(int vol = 50)
        {
            VolumeText = $"{{0}}% {LocalizationStrings.Volume}";
            Volume = Math.Clamp(vol, 0, 100);
        }

        public override void Deserialize(BinaryDeserializer binaryDeserializer)
        {
            Volume = binaryDeserializer.ReadInt();
        }

        public override void Serialize(BinarySerializer binarySerializer)
        {
            binarySerializer.WriteInt(Volume);
        }

        public float GetVolume() => Volume / 100f;

        public string GetString() => string.Format(VolumeText, Volume);
    }

    public class MusicEntry(string musicID = "") : ItemDataEntry, IHaveUIData
    {
        private string MusicName = "No music loaded";
        public string MusicID { private set; get; } = musicID;
        public int MusicIndex => MusicLoadManager.clips.Keys.ToList().IndexOf(MusicID);

        public void InitializeEntry()
        {if (string.IsNullOrWhiteSpace(MusicID) || MusicIndex == -1) TryUpdateMusicEntry(MusicLoadManager.clips.Keys.First() ?? ""); } 

        public override void Deserialize(BinaryDeserializer binaryDeserializer)
        {
            MusicID = binaryDeserializer.ReadString(Encoding.UTF8);
        }

        public bool TryUpdateMusicEntry(string musicID)
        {
            try
            {
                if (MusicLoadManager.clips.Count <= 0) return false;
                this.MusicID = musicID;
                SetDirty();
                UpdateMusicName();
                return true;
            }
            catch (IndexOutOfRangeException ex) { return false;}
        }
        /// <remarks>Unsafe, can cause wrong music selected when clip dictionary is updated</remarks>
        public bool TryUpdateMusicEntry(int musicIndex) => TryUpdateMusicEntry(MusicLoadManager.clips.Keys.ToArray()[((musicIndex) % MusicLoadManager.clips.Count)]);
        public void UpdateMusicEntry(string musicID) => TryUpdateMusicEntry(musicID);
        public void NextMusic() => TryUpdateMusicEntry(MusicLoadManager.clips.Keys.ToArray()[(MusicIndex + 1 + MusicLoadManager.clips.Count) % MusicLoadManager.clips.Count]);
        public void PreviousMusic() => TryUpdateMusicEntry(MusicLoadManager.clips.Keys.ToArray()[(MusicIndex - 1 + MusicLoadManager.clips.Count) % MusicLoadManager.clips.Count]);

        public override void Serialize(BinarySerializer binarySerializer)
        {
            binarySerializer.WriteString(MusicID, Encoding.UTF8);
        }

        public void UpdateMusicName()
        {
            if (MusicLoadManager.clips.Count > 0
                && MusicLoadManager.clips.ContainsKey(MusicID))
                MusicName = GetDisplayName(MusicLoadManager.clips[MusicID].name);
            else MusicName = "No music loaded";
        }

        public string GetString() => MusicName;

        private string GetDisplayName(string name)
        {
            int length;
            if ((length = name.LastIndexOf('.')) != -1)
            {
                name = name.Substring(0, length);
            }

            if (name.Length > 29) {
                name = name.Substring(0, 29);
                name = name + "...";
            }

            return name;
        }
    }
}
