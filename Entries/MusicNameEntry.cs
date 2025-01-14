using System;
using System.Linq;
using System.Text;
using FantomLis.BoomboxExtended.Locales;
using Zorro.Core.Serizalization;

namespace FantomLis.BoomboxExtended.Entries;

public class MusicNameEntry(string musicID = "") : BaseEntry, IHaveUIData
{
    public MusicNameEntry() : this("") {}
    private string MusicName = BoomboxLocalization.NoMusicLoaded;
    public string MusicID { private set; get; } = musicID;
    public int MusicIndex => MusicLoadManager.Music.Keys.ToList().IndexOf(MusicID);

    public void InitializeEntry()
    {if (string.IsNullOrWhiteSpace(MusicID) || MusicIndex == -1) TryUpdateMusicEntry(MusicLoadManager.Music.Keys.FirstOrDefault() ?? ""); } 

    public override void Deserialize(BinaryDeserializer binaryDeserializer)
    {
        MusicID = binaryDeserializer.ReadString(Encoding.UTF8);
    }

    public bool TryUpdateMusicEntry(string musicID)
    {
        try
        {
            if (MusicLoadManager.Music.Count <= 0) return false;
            this.MusicID = musicID;
            SetDirty();
            UpdateMusicName();
            return true;
        }
        catch (IndexOutOfRangeException ex) { return false;}
    }
    /// <remarks>Unsafe, can cause wrong music selected when clip dictionary is updated</remarks>
    public bool TryUpdateMusicEntry(int musicIndex) => TryUpdateMusicEntry(MusicLoadManager.Music.Keys.ToArray()[((musicIndex) % MusicLoadManager.Music.Count)]);
    public void UpdateMusicEntry(string musicID) => TryUpdateMusicEntry(musicID);
    public void NextMusic() => TryUpdateMusicEntry(MusicLoadManager.Music.Keys.ToArray()[(MusicIndex + 1 + MusicLoadManager.Music.Count) % MusicLoadManager.Music.Count]);
    public void PreviousMusic() => TryUpdateMusicEntry(MusicLoadManager.Music.Keys.ToArray()[(MusicIndex - 1 + MusicLoadManager.Music.Count) % MusicLoadManager.Music.Count]);

    public override void Serialize(BinarySerializer binarySerializer)
    {
        binarySerializer.WriteString(MusicID, Encoding.UTF8);
    }

    public void UpdateMusicName()
    {
        if (MusicLoadManager.Music.Count > 0
            && MusicLoadManager.Music.ContainsKey(MusicID))
            MusicName = GetDisplayName(MusicLoadManager.Music[MusicID].Name);
        else MusicName = BoomboxLocalization.NoMusicLoaded;
    }

    public string GetString() => MusicName;

    private string GetDisplayName(string name)
    {
        return name.Length > 29 ? name.Substring(0, 29)+ "..." : name;
    }
}