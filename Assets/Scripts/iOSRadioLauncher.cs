using System.Runtime.InteropServices;
using UnityEngine;

public static class iOSRadioLauncher
{
    [DllImport("__Internal")]
    private static extern string GetPlaybackState();

    public static string CheckiOSPlaybackState()
    {
#if UNITY_IOS && !UNITY_EDITOR
        return GetPlaybackState();
#else
        return "STOPPED";
#endif
    }

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

    [DllImport("__Internal")]
    private static extern void UpdateNowPlaying(string title, string artist);

    public static void SetNowPlaying(string title, string artist = null)
    {
#if UNITY_IOS && !UNITY_EDITOR
    UpdateNowPlaying(title, artist);
#endif
    }

}


