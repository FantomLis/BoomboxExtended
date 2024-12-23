using System;
using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FantomLis.BoomboxExtended.Settings;
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
                selectionScroll = Boombox.CurrentBoomboxMethod() == MusicSelectionMethodSetting.BoomboxMusicSelectionMethod.SelectionUIScroll 
                    ? selectionScroll
                    : GUI.BeginScrollView(new Rect(0,0, windowRect.width-40, windowRect.height-40), selectionScroll, 
                    new Rect(0,0,windowRect.width-80,h));
                var x = GUI.SelectionGrid(new Rect(0,0,windowRect.width-40, h), musicEntry.CurrentIndex, MusicLoadManager.clips.Keys.ToArray(), 1);
                TryUpdateMusic(x);
                GUI.EndScrollView();
                GUI.EndGroup();
            }
        }

        private bool TryUpdateMusic(int index)
        {
            if (index != musicEntry.CurrentIndex && musicEntry.TryUpdateMusicEntry(index))
            {
                timeEntry.currentTime = 0;
                timeEntry.SetDirty();
                Click.Play();
                lastChangeTime = Time.time;
                return true;
            }
            return false;
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
                switch (Boombox.CurrentBoomboxMethod())
                {
                    case MusicSelectionMethodSetting.BoomboxMusicSelectionMethod.SelectionUIScroll:
                    case MusicSelectionMethodSetting.BoomboxMusicSelectionMethod.SelectionUIMouse:
                    {
                        openUI = (Input.GetKey(KeyCode.Mouse1) || Player.localPlayer.input.aimIsPressed) && MusicLoadManager.clips.Count > 0 && isHeldByMe;
                        if (Boombox.CurrentBoomboxMethod() == MusicSelectionMethodSetting.BoomboxMusicSelectionMethod
                                .SelectionUIScroll)
                        {
                            Player.localPlayer.data.isInTitleCardTerminal = openUI;
                            if (openUI)
                            {
                                Cursor.lockState = CursorLockMode.None;
                                Cursor.visible = true;
                            }
                        }
                        if (Input.GetAxis("Mouse ScrollWheel") * 10 != 0  && lastChangeTime + 0.1f <= Time.time 
                                                                          && Boombox.CurrentBoomboxMethod() == 
                                                                          MusicSelectionMethodSetting.BoomboxMusicSelectionMethod.SelectionUIMouse
                                                                          && openUI)
                        {
                            var x =
                                (Mathf.RoundToInt(musicEntry.CurrentIndex + Input.GetAxis("Mouse ScrollWheel") * -1f * 10)+ MusicLoadManager.clips.Count) % MusicLoadManager.clips.Count;
                            TryUpdateMusic(x);
                            selectionScroll = Vector2.up * ((musicEntry.CurrentIndex * SongButtonSize * (Screen.height / 1080f))-(MusicLoadManager.clips.Count * SongButtonSize * Screen.height/1080f/2f));
                        }
                        break;
                    }
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

                    switch (Boombox.CurrentBoomboxMethod())
                    {
                        case MusicSelectionMethodSetting.BoomboxMusicSelectionMethod.Original:
                        default: 
                            if (Player.localPlayer.input.aimWasPressed) TryUpdateMusic(((musicEntry.CurrentIndex + 1) % MusicLoadManager.clips.Count));
                            break;
                        case MusicSelectionMethodSetting.BoomboxMusicSelectionMethod.ScrollWheel:
                            if (Input.GetAxis("Mouse ScrollWheel") * 10 != 0  && lastChangeTime + 0.1f <= Time.time)
                            {
                                TryUpdateMusic((Mathf.RoundToInt(musicEntry.CurrentIndex + Input.GetAxis("Mouse ScrollWheel") * -1f * 10)+ MusicLoadManager.clips.Count) % MusicLoadManager.clips.Count);
                            }
                            break;
                        case MusicSelectionMethodSetting.BoomboxMusicSelectionMethod.SelectionUIScroll: 
                        case MusicSelectionMethodSetting.BoomboxMusicSelectionMethod.Default:
                        case MusicSelectionMethodSetting.BoomboxMusicSelectionMethod.SelectionUIMouse: 
                            if (openUI)
                            {
                                if (Player.localPlayer.input.aimWasPressed) Click.Play();
                                if (MusicLoadManager.clips.Count <= 0)
                                {
                                    HelmetText.Instance.SetHelmetText("No Music", 2f); // TODO: To Alert utils
                                }
                            }
                            break;
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

            Music.volume = volumeEntry.GetVolume();
            musicEntry.UpdateMusicName();

            bool flag = onOffEntry.on;
            if (flag != Music.isPlaying)
            {
                if (flag && checkMusic(musicEntry.SelectMusicID))
                {
                    Music.clip = MusicLoadManager.clips[musicEntry.SelectMusicID];
                    Music.time = timeEntry.currentTime;
                    Music.Play();
                }
                else
                {
                    Music.Stop();
                }
            }

            batteryEntry.m_charge -= (flag && Boombox.BatteryCapacity.Value >= 0 && batteryEntry.m_charge >= 0f ? Time.deltaTime : 0);
            timeEntry.currentTime = Music.time;
        }

        public static bool checkMusic(string id)    
        {
            return MusicLoadManager.clips.ContainsKey(id);
        }
    }

    public class VolumeEntry : ItemDataEntry, IHaveUIData
    {
        public int Volume { get; private set; }

        private string VolumeLanguage;

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
            VolumeLanguage = $"{{0}}% {LocalizationStrings.Volume}";
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

        public float GetVolume()
        {
            return Volume / 100f;
        }

        public string GetString()
        {
            return string.Format(VolumeLanguage, Volume);
        }
    }

    public class MusicEntry : ItemDataEntry, IHaveUIData
    {
        private string MusicName = String.Empty;
        public int CurrentIndex { private set; get; }
        public string SelectMusicID { private set; get; }

        public override void Deserialize(BinaryDeserializer binaryDeserializer)
        {
            SelectMusicID = binaryDeserializer.ReadString(Encoding.UTF8);
            CurrentIndex = binaryDeserializer.ReadInt();
        }

        public bool TryUpdateMusicEntry(int MusicIndex)
        {
            try
            {
                if (MusicLoadManager.clips.Count <= 0) return false;
                CurrentIndex = MusicIndex;
                SelectMusicID = MusicLoadManager.clips.Keys.ToArray()[((CurrentIndex) % MusicLoadManager.clips.Count)];
                UpdateMusicName();
                SetDirty();
                return true;
            }
            catch (IndexOutOfRangeException ex) { return false;}
        }

        public void UpdateMusicEntry(string MusicID)
        {
            SelectMusicID = MusicID;
            UpdateMusicName();
            SetDirty();
        }
        
        public void UpdateMusicEntry(int MusicIndex)
        {
            CurrentIndex = MusicIndex;
            SelectMusicID = MusicLoadManager.clips.Keys.ToArray()[((CurrentIndex) % MusicLoadManager.clips.Count)];
            UpdateMusicName();
            SetDirty();
        }

        public override void Serialize(BinarySerializer binarySerializer)
        {
            binarySerializer.WriteString(SelectMusicID, Encoding.UTF8);
            binarySerializer.WriteInt(CurrentIndex);
        }

        public void UpdateMusicName()
        {
            MusicName = string.Empty;

            if (MusicLoadManager.clips.Count > 0 && BoomboxBehaviour.checkMusic(SelectMusicID))
            {
                MusicName = GetDisplayName(MusicLoadManager.clips[SelectMusicID].name);
            }
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
