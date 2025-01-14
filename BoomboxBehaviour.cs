using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FantomLis.BoomboxExtended.Entries;
using FantomLis.BoomboxExtended.Locales;
using FantomLis.BoomboxExtended.Settings;
using FantomLis.BoomboxExtended.Utils;
using Photon.Pun;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace FantomLis.BoomboxExtended
{
    public class BoomboxBehaviour : ItemInstanceBehaviour
    {
        private const float uiSize = 0.4f;
        private const float SongButtonSize = 25f;

        private BatteryEntry batteryEntry;
        private OnOffEntry onOffEntry;
        private TimeEntry timeEntry;
        private VolumeEntry volumeEntry;
        private MusicNameEntry musicEntry;
        private PlayerEntry _lengthEntry;

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
            if (MusicLoadManager.clips.TryGetValue(musicEntry.MusicID, out var c))
            {
                timeEntry.currentTime = 0;
                timeEntry.SetDirty();
                Click.Play();
                lastChangeTime = Time.time;
                musicEntry.UpdateMusicName();
                Music.clip = c;
                _lengthEntry.UpdateLenght(c.length);
                _lengthEntry.UpdateLenght(Music.clip.length);
            Music.time = timeEntry.currentTime;}
        }

        void Awake()
        {
            Click = transform.Find("SFX/Click").GetComponent<SFX_PlayOneShot>();
            Music = GetComponentInParent<AudioSource>();
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
                musicEntry = new MusicNameEntry();
                data.AddDataEntry(musicEntry);
            }
            
            if (!data.TryGetEntry(out _lengthEntry))
            {
                _lengthEntry = new PlayerEntry();
                data.AddDataEntry(_lengthEntry);
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
                                HelmetText.Instance.SetHelmetText(BoomboxLocalization.NoMusicLoaded, 2f); 
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
                                HelmetText.Instance.SetHelmetText(BoomboxLocalization.NoMusicLoaded, 2f); 
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
                                HelmetText.Instance.SetHelmetText(BoomboxLocalization.NoMusicLoaded, 2f); 
                                break;
                            }
                            if (Input.GetAxis("Mouse ScrollWheel") * 10 * -1 >= 1) NextMusic();
                            else if (Input.GetAxis("Mouse ScrollWheel") * 10 * -1 <= -1) PreviousMusic();
                        }
                        selectionScroll = Vector2.up * ((musicEntry.MusicIndex * SongButtonSize * (Screen.height / 1080f))-(MusicLoadManager.clips.Count * SongButtonSize * Screen.height/1080f/2f));
                        break;
                    case MusicSelectionMethodSetting.BoomboxMusicSelectionMethod.ScrollWheel:
                        if (Player.localPlayer.HasLockedInput()) break;
                        if (Input.GetAxis("Mouse ScrollWheel") * 10 * -1 != 0  && lastChangeTime + 0.1f // <<< - TODO: to config parameter
                            <= Time.time)
                        {
                            if (Player.localPlayer.input.aimWasPressed) Click.Play();
                            if (MusicLoadManager.clips.Count <= 0)
                            {
                                HelmetText.Instance.SetHelmetText(BoomboxLocalization.NoMusicLoaded, 2f); 
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
                            HelmetText.Instance.SetHelmetText(BoomboxLocalization.NoMusicLoaded, 2f);
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
                        volumeEntry.PlusVolume(Boombox.VolumeStep.Value);

                        Click.Play();
                    }
                    if (GlobalInputHandler.GetKeyUp(Boombox.VolumeDownKey.Keycode()))
                    {
                        volumeEntry.MinusVolume(Boombox.VolumeStep.Value);
                        Click.Play();
                    }
                }

                musicEntry.UpdateMusicName();
                if (MusicLoadManager.clips.TryGetValue(musicEntry.MusicID, out var _c)) {
                    _lengthEntry.UpdateLenght(_c.length); 
                    if (_c == Music.clip) _lengthEntry.UpdateCurrentPosition(Music.time);
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
                    if (!Boombox.CurrentPauseMusic) Music.Stop();
                    else Music.Pause();
                }
            }

            #endregion

            #region Update boombox

            batteryEntry.m_charge -= (flag && Boombox.BatteryCapacity.Value >= 0 && batteryEntry.m_charge >= 0f ? Time.deltaTime : 0);
            timeEntry.currentTime = Music.time;
            Music.volume = volumeEntry.GetVolume();

            #endregion
        }

        public void UpdateEquippedUI()
        {
            if (!isHeldByMe) return;
            if (Player.localPlayer.TryGetInventory(out var o))
            {
                var x = o.GetItems().Find(x => x.item.Equals(this.itemInstance.item));
                UserInterface.Instance.equippedUI.SetData(ItemDescriptor.Empty); // Clear Equipped UI before redrawing boombox Tooltips
                UserInterface.Instance.equippedUI.SetData(x);                    // because of itemDescriptor.data != this.m_lastItemDescriptor.data
            }
        }
    }
}
