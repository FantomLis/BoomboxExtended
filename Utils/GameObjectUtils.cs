using UnityEngine;

namespace FantomLis.BoomboxExtended.Utils;

public sealed class GameObjectUtils : MonoBehaviour
{
    private static GameObject? DontDestroyOnLoad;
    public static GameObject MakeNewDontDestroyOnLoad(string name)
    {
        var x = new GameObject(name);
        DontDestroyOnLoad(x);
        return x;
    }

    public static GameObject GetDefaultDontDestroyOnLoad() =>
        DontDestroyOnLoad ??= MakeNewDontDestroyOnLoad("DontDestroyOnLoad Object");
}