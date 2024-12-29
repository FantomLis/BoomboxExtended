using System;
using Zorro.Core.Serizalization;

namespace FantomLis.BoomboxExtended.Entries;

public class VolumeEntry : BaseEntry, IHaveUIData
{
    public VolumeEntry(){}
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
        VolumeText = $"{{0}}% {BoomboxLocalization.BoomboxVolume}";
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