using System;
using System.Text;
using UnityEngine;

namespace FantomLis.BoomboxExtended.Utils;

public static class LogUtils
{
    public static readonly string LogSource = Boombox.ItemName;
    public static bool IsDebug => Boombox.IsDebug;

    public enum LogType
    {
        Debug,
        Info,
        Warning,
        Error,
        Fatal
    }

    public static void Log(LogType type, string text)
    {
        if (type == LogType.Debug && !IsDebug) return;
        StringBuilder b = new();
        b.Append($"[{LogSource}]");
        b.Append(type.ToString());
        b.Append(text);
        switch (type)
        {
            case LogType.Debug:
            case LogType.Info:
                Debug.Log(b.ToString());
                break;
            case LogType.Warning:
                Debug.LogWarning(b.ToString());
                break;
            case LogType.Error:
                Debug.LogError(b.ToString());
                break;
            case LogType.Fatal:
                Debug.LogError(b.ToString());
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    public static void LogDebug(string text) => Log(LogType.Debug, text);
    public static void LogInfo(string text) => Log(LogType.Info, text);
    public static void LogWarning(string text) => Log(LogType.Warning, text);
    public static void LogError(string text) => Log(LogType.Error, text);
    public static void LogFatal(string text) => Log(LogType.Fatal, text);
    public static void LogFatal(Exception ex) => LogError(ex);

    public static void LogError(Exception ex) => Debug.LogError(ex);
}