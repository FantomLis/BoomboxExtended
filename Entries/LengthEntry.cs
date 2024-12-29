using System;
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
        if (Lenght <= 0) throw new ArgumentException($"Length should be more than 0.");
        Lenght = length;
    }
    
    public void UpdateCurrentPosition(float pos)
    {
        if (Lenght < 0) throw new ArgumentException($"Position should be more or equals 0.");
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

    public override byte ID() => 13;

    public string GetString() => $"Test: {GetPercent*100f:0}";
}