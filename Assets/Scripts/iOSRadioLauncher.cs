using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class iOSRadioLauncher : MonoBehaviour
{
    private float playbackTime = 0;
    public static string cachedNowPlaying = "Streaming...";
    public TMP_Text debugTextforIOSState;

    private void Start()
    {
        Debug.Log("[IOSRADIOLAUNCHER] Start: " + CheckiOSPlaybackState());
    }

    void Update()
    {
#if UNITY_IOS && !UNITY_EDITOR
            string state = iOSRadioLauncher.CheckiOSPlaybackState();
            debugTextforIOSState.text = state;
            if (state == "PLAYING")
            {
                playbackTime += Time.deltaTime;
            }
            else if (state == "STOPPED" || state == "BUFFERING" || state == "ERROR")
            {
                playbackTime = 0f;
            }
#endif
    }

#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern IntPtr GetLastPlaybackError();
#endif

    public static string GetLastPlaybackErrorMessage()
    {
#if UNITY_IOS && !UNITY_EDITOR
    IntPtr ptr = GetLastPlaybackError();
    return Marshal.PtrToStringUTF8(ptr);
#else
        return "Unavailable in Editor";
#endif
    }

    [DllImport("__Internal")]
    private static extern IntPtr GetNowPlayingText();

    public static string GetiOSNowPlaying()
    {
#if UNITY_IOS && !UNITY_EDITOR
    IntPtr ptr = GetNowPlayingText();
    return Marshal.PtrToStringUTF8(ptr);
#else
        return "Streaming...";
#endif
    }


    [Serializable]
    private class NowPlayingWrapper
    {
        public string now_playing;
    }

    public static void StartNativeStream(string url, string stationName, Texture2D favicon)
    {
        Debug.Log("[iOSRadioLauncher] StartNativeStream called");
#if UNITY_IOS && !UNITY_EDITOR
            byte[] bytes = favicon.EncodeToPNG();
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                IntPtr ptr = handle.AddrOfPinnedObject();
                StartStreamWithArtwork_Internal(url, stationName, ptr, bytes.Length);
            }
            finally
            {
                handle.Free();
            }

#endif
    }

    [DllImport("__Internal")]
    private static extern void StartStreamWithArtwork_Internal(string url, string stationName, IntPtr artwork, int length);

    [DllImport("__Internal")]
    private static extern float GetBufferingPercent();

    public static float GetiOSBufferingPercent()
    {
#if UNITY_IOS && !UNITY_EDITOR
    return GetBufferingPercent();
#else
        return 0f;
#endif
    }

    public string GetiOSPlaybackTime()
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(playbackTime);
        return string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
    }


    [DllImport("__Internal")]
    private static extern System.IntPtr GetPlaybackState();

    public static string CheckiOSPlaybackState()
    {
#if UNITY_IOS && !UNITY_EDITOR
    IntPtr strPtr = GetPlaybackState();
    return Marshal.PtrToStringAnsi(strPtr);
#else
        return "STOPPED";
#endif
    }

    [DllImport("__Internal")]
    private static extern void StopStream();

    public static void StopNativeStream()
    {
#if UNITY_IOS && !UNITY_EDITOR
            StopStream();
#endif
    }




}


