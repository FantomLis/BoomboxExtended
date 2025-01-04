using System;
using System.Text;
using FantomLis.BoomboxExtended.Utils;
using Zorro.Core.Serizalization;

namespace FantomLis.BoomboxExtended.Entries;

public class PlayerEntry: BaseEntry, IHaveUIData
{
    public PlayerEntry() {}
    public float Lenght { get; private set; }
    public float Position { get; private set; }
    public float GetPercent => Position / Lenght;
    public void UpdateLenght(float length)
    {
        if (length <= 0) throw new ArgumentException($"Length should be more than 0.");
        Lenght = length;
    }
    
    public void UpdateCurrentPosition(float pos)
    {
        if (pos < 0) throw new ArgumentException($"Position should be more or equals 0.");
        Position = pos;
    }

    public override void Serialize(BinarySerializer binarySerializer)
    {
        binarySerializer.WriteFloat(Lenght);
        binarySerializer.WriteFloat(Position);
    }

    public override void Deserialize(BinaryDeserializer binaryDeserializer)
    {
        Lenght = binaryDeserializer.ReadFloat();
        Position = binaryDeserializer.ReadFloat();
    }

    
    public string GetString() => $"{TimeUtils.ToMinSecTime(Position)}/{TimeUtils.ToMinSecTime(Lenght)}";
}