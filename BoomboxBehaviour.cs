using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Zorro.Core.Serizalization;
namespace FantomLis.BoomboxExtended
{
    public class BoomboxBehaviour : ItemInstanceBehaviour
    {
        public static Dictionary<string, AudioClip> clips = new ();

        private BatteryEntry batteryEntry;
        private OnOffEntry onOffEntry;
        private TimeEntry timeEntry;
        private VolumeEntry volumeEntry;
        private MusicEntry musicEntry;

        private SFX_PlayOneShot Click;
        private AudioSource Music;
        
        private string currentId;

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
                    selectMusicId = MusicManager.AudioClips.Keys.FirstOrDefault() ?? string.Empty,
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
            if (isHeldByMe && !Player.localPlayer.HasLockedInput())
            {
                if (Player.localPlayer.input.clickWasPressed)
                {
                    if (MusicManager.AudioClips.Count == 0) 
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

                if (Player.localPlayer.input.aimWasPressed)
                {
                    if (MusicManager.AudioClips.Count > 0)
                    {
                        musicEntry.selectMusicId = MusicManager.AudioClips.Keys.ToArray()[((++musicEntry.currentIndex) % MusicManager.AudioClips.Count)];
                        musicEntry.UpdateMusicName();
                        musicEntry.SetDirty();

                        timeEntry.currentTime = 0;
                        timeEntry.SetDirty();
                    }

                    Click.Play();
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
                        Music.clip = MusicManager.AudioClips[musicEntry.selectMusicId];
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
            return MusicManager.AudioClips.ContainsKey(id);
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

            if (MusicManager.AudioClips.Count > 0 && BoomboxBehaviour.checkMusic(selectMusicId))
            {
                MusicName = getMusicName(MusicManager.AudioClips[selectMusicId].name);
            }
        }

        public string GetString()
        {
            return MusicName;
        }

        private string getMusicName(string name)
        {
            int length;
            name = MusicManager.DehashAudioClipName(name);
            if ((length = name.LastIndexOf('.')) != -1)
            {
                name = name.Substring(0, length);
            }

            if (name.Length > 25) {
                name = name.Substring(0, 25);
                name = name + "...";
            }
            
            return name;
        }
    }
}
