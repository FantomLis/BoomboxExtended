namespace FantomLis.BoomboxExtended.Utils;

public static class TimeUtils
{
    public static string ToMinSecTime(float time)
    {
        return $"{(int)time / 60:00}:{(int)time % 60:00}";
    }
}