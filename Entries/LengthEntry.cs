using System;
using System.Text;
using Zorro.Core.Serizalization;

namespace FantomLis.BoomboxExtended.Entries;

public class LengthEntry: BaseEntry, IHaveUIData
{
    public LengthEntry() {}
    public float Lenght { get; private set; }
    public float CurrentPosition { get; private set; }
    public float GetPercent => CurrentPosition / Lenght;
    public void UpdateLenght(float length)
    {
        if (length <= 0) throw new ArgumentException($"Length should be more than 0.");
        Lenght = length;
    }
    
    public void UpdateCurrentPosition(float pos)
    {
        if (pos < 0) throw new ArgumentException($"Position should be more or equals 0.");
        CurrentPosition = pos;
    }

    public override void Serialize(BinarySerializer binarySerializer)
    {
        binarySerializer.WriteFloat(Lenght);
        binarySerializer.WriteFloat(CurrentPosition);
    }

    public override void Deserialize(BinaryDeserializer binaryDeserializer)
    {
        Lenght = binaryDeserializer.ReadFloat();
        CurrentPosition = binaryDeserializer.ReadFloat();
    }

    private string ToMinSecTime(float time)
    {
        return $"{(int)time / 60:00}:{(int)time % 60:00}";
    }
    public string GetString() => $"{ToMinSecTime(CurrentPosition)}/{ToMinSecTime(Lenght)}";
}