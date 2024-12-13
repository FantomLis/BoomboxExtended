using System;
using System.Collections;
using UnityEngine;

namespace FantomLis.BoomboxExtended;

public class LoadingManager : MonoBehaviour
{
    public static Action? BoomboxPreLoadDone;
    public IEnumerator AwaitForBoomboxCreation()
    {
        yield return new WaitUntil(() => Boombox.Self != null);
        BoomboxPreLoadDone?.Invoke();
    }
}