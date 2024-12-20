﻿using System;
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
        public static Dictionary<string, AudioClip> clips = new ();
        private static readonly float uiSize = 0.3f;
        private static float SongButtonSize;

        private BatteryEntry batteryEntry;
        private OnOffEntry onOffEntry;
        private TimeEntry timeEntry;
        private VolumeEntry volumeEntry;
        private MusicEntry musicEntry;

        private SFX_PlayOneShot Click;
        private AudioSource Music;
        
        private string currentId;
        private float lastChangeTime;
        private bool openUI;
        Rect windowRect = new Rect((Screen.width - Screen.width * uiSize)/2f , (Screen.height - Screen.height * uiSize)/2f , Screen.width*uiSize, Screen.height*uiSize);
        private void OnGUI()
        {
            if (isHeldByMe && openUI)
            {
                GUI.BeginGroup(windowRect);
                SongButtonSize = 25f;
                selectionScroll = GUI.BeginScrollView(new Rect(0,0, Screen.width*uiSize-40, Screen.height*uiSize-40), selectionScroll, new Rect(0,0,Screen.width*uiSize-40-40, clips.Count * SongButtonSize * Screen.height/1080f));
                var x = musicEntry.currentIndex;
                musicEntry.currentIndex = GUI.SelectionGrid(new Rect(0,0,Screen.width*uiSize-40, clips.Count * SongButtonSize * Screen.height/1080f), musicEntry.currentIndex, clips.Keys.ToArray(), 1);
                if (x != musicEntry.currentIndex)
                {
                    musicEntry.selectMusicId = clips.Keys.ToArray()[((musicEntry.currentIndex) % clips.Count)];
                    musicEntry.UpdateMusicName();
                    musicEntry.SetDirty();

                    timeEntry.currentTime = 0;
                    timeEntry.SetDirty();
                }
                GUI.EndScrollView();
                GUI.EndGroup();
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        private Vector2 selectionScroll = new Vector2();

        void Awake()
        {
            Click = transform.Find("SFX/Click").GetComponent<SFX_PlayOneShot>();
            Music = GetComponent<AudioSource>();
            var gameHandler = GameHandler.Instance;
            if (gameHandler != null)
            {
                var sfxVolumeSetting = gameHandler.SettingsHandler.GetSetting<SFXVolumeSetting>();
                if (sfxVolumeSetting != null)
                {
                    Music.outputAudioMixerGroup = sfxVolumeSetting.mixerGroup;
                }
            }
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
                musicEntry = new MusicEntry()
                {
                    selectMusicId = clips.Keys.FirstOrDefault() ?? string.Empty,
                };

                data.AddDataEntry(musicEntry);
            }

            if (!data.TryGetEntry(out volumeEntry))
            {
                volumeEntry = new VolumeEntry()
                {
                    volume = 5
                };

                data.AddDataEntry(volumeEntry);
            }

            musicEntry.UpdateMusicName();
        }

        void Update()
        {
            if (isHeldByMe)
            {
                switch (Boombox.CurrentBoomboxMethod())
                {
                    case MusicSelectionMethodSetting.BoomboxMusicSelectionMethod.SelectionUI:
                    case MusicSelectionMethodSetting.BoomboxMusicSelectionMethod.SelectionUINoScroll:
                    {
                        openUI = (Input.GetKey(KeyCode.Mouse1) || Player.localPlayer.input.aimIsPressed) && clips.Count > 0;
                        Player.localPlayer.data.isInTitleCardTerminal = openUI;
                        break;
                    }
                }
            }
            if (isHeldByMe && !Player.localPlayer.HasLockedInput())
            {
                if (Player.localPlayer.input.clickWasPressed)
                {
                    if (clips.Count == 0) 
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
                    {
                        if (Player.localPlayer.input.aimWasPressed)
                        {
                            if (clips.Count > 0)
                            {
                                musicEntry.selectMusicId = clips.Keys.ToArray()[((++musicEntry.currentIndex) % clips.Count)];
                                musicEntry.UpdateMusicName();
                                musicEntry.SetDirty();

                                timeEntry.currentTime = 0;
                                timeEntry.SetDirty();
                            }

                            Click.Play();
                        }
                        break;
                    }
                    case MusicSelectionMethodSetting.BoomboxMusicSelectionMethod.ScrollWheel:
                        if (Input.GetAxis("Mouse ScrollWheel") * 10 != 0  && lastChangeTime + 0.1f <= Time.time)
                        {
                            var x = musicEntry.currentIndex;
                            musicEntry.currentIndex =
                                (Mathf.RoundToInt(musicEntry.currentIndex + Input.GetAxis("Mouse ScrollWheel") * 10)+ clips.Count) % clips.Count;
                            if (clips.Count > 0)
                            {
                                musicEntry.selectMusicId =
                                    clips.Keys.ToArray()[musicEntry.currentIndex];
                                musicEntry.UpdateMusicName();
                                musicEntry.SetDirty();

                                timeEntry.currentTime = 0;
                                timeEntry.SetDirty();
                            }

                            if (x != musicEntry.currentIndex)
                            {
                                lastChangeTime = Time.time;
                                Click.Play();
                            }
                        }
                        break;
                    case MusicSelectionMethodSetting.BoomboxMusicSelectionMethod.SelectionUI: 
                    case MusicSelectionMethodSetting.BoomboxMusicSelectionMethod.Default:
                        if (openUI)
                        {
                            if (Player.localPlayer.input.aimWasPressed) Click.Play();
                            if (clips.Count <= 0)  {HelmetText.Instance.SetHelmetText("No Music", 2f);
                                break;
                            }
                            if (Input.GetAxis("Mouse ScrollWheel") * 10 != 0  && lastChangeTime + 0.1f <= Time.time)
                            {
                                var x = musicEntry.currentIndex;
                                musicEntry.currentIndex =
                                    (Mathf.RoundToInt(musicEntry.currentIndex + Input.GetAxis("Mouse ScrollWheel") * 10)+ clips.Count) % clips.Count;
                                if (clips.Count > 0)
                                {
                                    musicEntry.selectMusicId =
                                        clips.Keys.ToArray()[musicEntry.currentIndex];
                                    musicEntry.UpdateMusicName();
                                    musicEntry.SetDirty();

                                    timeEntry.currentTime = 0;
                                    timeEntry.SetDirty();
                                }

                                if (x != musicEntry.currentIndex)
                                {
                                    lastChangeTime = Time.time;
                                    Click.Play();
                                }
                            }
                        }
                        break;
                    case MusicSelectionMethodSetting.BoomboxMusicSelectionMethod.SelectionUINoScroll: 
                        if (openUI)
                        {
                            if (Player.localPlayer.input.aimWasPressed) Click.Play();
                            if (clips.Count <= 0)  {HelmetText.Instance.SetHelmetText("No Music", 2f); }
                        }
                        break;
                }

                if (GlobalInputHandler.GetKeyUp(Boombox.VolumeUpKey.Keycode()))
                {
                    if (volumeEntry.volume <= 9) {
                        volumeEntry.volume += 1;
                        volumeEntry.SetDirty();
                    }

                    Click.Play();
                }

                if (GlobalInputHandler.GetKeyUp(Boombox.VolumeDownKey.Keycode()))
                {
                    if (volumeEntry.volume >= 1) {
                        volumeEntry.volume -= 1;
                        volumeEntry.SetDirty();
                    } 

                    Click.Play();
                }
            }

            if (Boombox.BatteryCapacity.Value >= 0) {
                if (batteryEntry.m_charge < 0f)
                {
                    onOffEntry.on = false;
                }
            }

            Music.volume = volumeEntry.GetVolume();

            bool flag = onOffEntry.on;
            if (flag != Music.isPlaying || currentId != musicEntry.selectMusicId)
            {
                currentId = musicEntry.selectMusicId;

                if (flag)
                {
                    if (checkMusic(musicEntry.selectMusicId))
                    {
                        Music.clip = clips[musicEntry.selectMusicId];
                        Music.time = timeEntry.currentTime;
                        Music.Play();
                    }
                }
                else
                {
                    Music.Stop();
                }
            }

            if (flag)
            {
                if (Boombox.BatteryCapacity.Value > 0) {
                    batteryEntry.m_charge -= Time.deltaTime;
                }

                timeEntry.currentTime = Music.time;
            }
        }

        public static bool checkMusic(string id)    
        {
            return clips.ContainsKey(id);
        }
    }

    public class VolumeEntry : ItemDataEntry, IHaveUIData
    {
        public int volume;
        public int max_volume = 10;

        private string VolumeLanguage = "{0}% Volume";

        public VolumeEntry()
        {
            VolumeLanguage = $"{{0}}% {LocalizationStrings.Volume}";
        }

        public override void Deserialize(BinaryDeserializer binaryDeserializer)
        {
            volume = binaryDeserializer.ReadInt();
        }

        public override void Serialize(BinarySerializer binarySerializer)
        {
            binarySerializer.WriteInt(volume);
        }

        public float GetVolume()
        {
            return volume / 10f;
        }

        public string GetString()
        {
            return string.Format(VolumeLanguage, volume * 10);
        }
    }

    public class MusicEntry : ItemDataEntry, IHaveUIData
    {
        private string MusicName;
        public int currentIndex = 0;
        public string selectMusicId;

        public override void Deserialize(BinaryDeserializer binaryDeserializer)
        {
            selectMusicId = binaryDeserializer.ReadString(Encoding.UTF8);
            currentIndex = binaryDeserializer.ReadInt();
        }

        public override void Serialize(BinarySerializer binarySerializer)
        {
            binarySerializer.WriteString(selectMusicId, Encoding.UTF8);
            binarySerializer.WriteInt(currentIndex);
        }

        public void UpdateMusicName()
        {
            MusicName = string.Empty;

            if (BoomboxBehaviour.clips.Count > 0 && BoomboxBehaviour.checkMusic(selectMusicId))
            {
                MusicName = getMusicName(BoomboxBehaviour.clips[selectMusicId].name);
            }
        }

        public string GetString()
        {
            return MusicName;
        }

        private string getMusicName(string name)
        {
            int length;
            if ((length = name.LastIndexOf('.')) != -1)
            {
                name = name.Substring(0, length);
            }

            if (name.Length > 15) {
                name = name.Substring(0, 15);
                name = name + "...";
            }

            return name;
        }
    }
}
