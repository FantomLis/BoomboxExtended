using System;
using FantomLis.BoomboxExtended.Locales;
using Zorro.Core.Serizalization;

namespace FantomLis.BoomboxExtended.Entries;

public class VolumeEntry : BaseEntry, IHaveUIData
{
    public VolumeEntry() : this (50){}
    public float Volume { get; private set; }

    private string VolumeText = $"{{0}}% {BoomboxLocalization.BoomboxVolume}";

    /// <summary>
    /// Updates volume
    /// </summary>
    /// <param name="vol">Volume from 0 to 100</param>
    public void UpdateVolume(float vol)
    {
        Volume = Math.Clamp(vol, 0, 100);
        SetDirty();
    }

    /// <summary>
    /// Decrease volume
    /// </summary>
    /// <param name="vol">How much decrease volume</param>
    public void MinusVolume(float vol) => UpdateVolume(Volume - vol);
    /// <summary>
    /// Increase volume
    /// </summary>
    /// <param name="vol">How much increase volume</param>
    public void PlusVolume(float vol) => UpdateVolume(Volume + vol);

    public VolumeEntry(float vol = 50)
    {
        Volume = Math.Clamp(vol, 0, 100);
    }

    public override void Deserialize(BinaryDeserializer binaryDeserializer)
    {
        Volume = binaryDeserializer.ReadFloat();
    }

    public override void Serialize(BinarySerializer binarySerializer)
    {
        binarySerializer.WriteFloat(Volume);
    }

    public float GetVolume() => Volume / 100f;

    public string GetString() => string.Format(VolumeText, $"{Volume:f0}");
}