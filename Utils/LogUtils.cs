using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

namespace FantomLis.BoomboxExtended.Utils;

public static class LogUtils
{
    public static readonly string LogSource = Boombox.ItemName;
    private static __LogObject __logObject = GameObjectUtils.GetDefaultDontDestroyOnLoad().AddComponent<__LogObject>();
    private class __LogObject : MonoBehaviour
    {
        public List<Action> __logActionList = new();
        public Thread __realUnityThread = Thread.CurrentThread;
        public void Update()
        {
            var __tmp_logList = __logActionList.ToArray();
            foreach (var act in __tmp_logList)
            {
                act?.Invoke();
                __logActionList.Remove(act);
            }
        }
    }

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
#if BepInEx 
        switch (type)
        {
            case LogType.Debug:
                Boombox.Log.LogDebug(text);
                break;
            case LogType.Info:
                Boombox.Log.LogInfo(text);
                break;
            case LogType.Warning:
                Boombox.Log.LogWarning(text);
                break;
            case LogType.Error:
                Boombox.Log.LogError(text);
                break;
            case LogType.Fatal:
                Boombox.Log.LogFatal(text);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
#else
        if (!object.ReferenceEquals(System.Threading.Thread.CurrentThread, __logObject.__realUnityThread))
            __logObject.__logActionList.Add(() => LogText(type, text));
        else LogText(type, text);
#endif
    }

    private static void LogText(LogType type, string text)
    {
        if (type == LogType.Debug && !Boombox.IsDebug) return;
        StringBuilder b = new();
        b.Append($"[{LogSource}:{type.ToString()}] ");
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