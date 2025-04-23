using System.Runtime.InteropServices;
using UnityEngine;

public static class iOSRadioLauncher
{
    [DllImport("__Internal")]
    private static extern void StartStream(string url);

    [DllImport("__Internal")]
    private static extern void StopStream();

    public static void StartNativeStream(string url)
    {
#if UNITY_IOS && !UNITY_EDITOR
        StartStream(url);
#endif
    }

    public static void StopNativeStream()
    {
#if UNITY_IOS && !UNITY_EDITOR
        StopStream();
#endif
    }
}
