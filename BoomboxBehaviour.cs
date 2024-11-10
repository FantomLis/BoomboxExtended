using Photon.Pun;
using ShopUtils;
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

        private int currentIndex = 0;
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
            if (!Boombox.InfiniteBattery)
            {
                if (!data.TryGetEntry(out batteryEntry))
                {
                    batteryEntry = new BatteryEntry()
                    {
                        m_charge = Boombox.BatteryCapacity,
                        m_maxCharge = Boombox.BatteryCapacity
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

                if (Player.localPlayer.input.aimWasPressed)
                {
                    if (clips.Count > 0)
                    {
                        musicEntry.selectMusicId = clips.Keys.ToArray()[((++currentIndex) % clips.Count)];
                        musicEntry.UpdateMusicName();
                        musicEntry.SetDirty();

                        timeEntry.currentTime = 0;
                        timeEntry.SetDirty();
                    }

                    Click.Play();
                }

                if (GlobalInputHandler.GetKeyUp(Boombox.VolumeUpKey.Value))
                {
                    if (volumeEntry.volume <= 9) {
                        volumeEntry.volume += 1;
                        volumeEntry.SetDirty();
                    }

                    Click.Play();
                }

                if (GlobalInputHandler.GetKeyUp(Boombox.VolumeDownKey.Value))
                {
                    if (volumeEntry.volume >= 1) {
                        volumeEntry.volume -= 1;
                        volumeEntry.SetDirty();
                    } 

                    Click.Play();
                }
            }

            if (!Boombox.InfiniteBattery) {
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
                if (!Boombox.InfiniteBattery) {
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
            Languages.TryGetLanguage("BoomboxVolume", out VolumeLanguage);
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

        public string selectMusicId;

        public override void Deserialize(BinaryDeserializer binaryDeserializer)
        {
            selectMusicId = binaryDeserializer.ReadString(Encoding.UTF8);
        }

        public override void Serialize(BinarySerializer binarySerializer)
        {
            binarySerializer.WriteString(selectMusicId, Encoding.UTF8);
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
