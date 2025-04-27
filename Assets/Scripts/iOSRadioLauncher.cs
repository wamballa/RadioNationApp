using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class iOSRadioLauncher : MonoBehaviour
{
    private float playbackTime;

    //     void Update()
    //     {
    // #if UNITY_IOS && !UNITY_EDITOR
    //         string state = iOSRadioLauncher.CheckiOSPlaybackState();

    //         if (state == "PLAYING")
    //         {
    //             playbackTime += Time.deltaTime;
    //         }
    //         else if (state == "STOPPED" || state == "BUFFERING" || state == "ERROR")
    //         {
    //             playbackTime = 0f;
    //         }
    // #endif
    //     }

    public string GetiOSPlaybackTime()
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(playbackTime);
        return string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
    }

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
    private static extern void StopStream();

    public static void StopNativeStream()
    {
#if UNITY_IOS && !UNITY_EDITOR
            StopStream();
#endif
    }


    //     // [DllImport("__Internal")]
    //     // private static extern void StartStreamWithArtwork(string url, string stationName, byte[] artwork, int length);


    //     public static void StartNativeStream(string url, string stationName, Texture2D favicon)
    //     {
    //         Debug.Log("StartNativeStream called");
    // #if UNITY_IOS && !UNITY_EDITOR
    //         byte[] bytes = favicon.EncodeToPNG();
    //         GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
    //         try
    //         {
    //             IntPtr ptr = handle.AddrOfPinnedObject();
    //             StartStreamWithArtwork_Internal(url, stationName, ptr, bytes.Length);
    //         }
    //         finally
    //         {
    //             handle.Free();
    //         }

    // #endif
    //     }

    //     [DllImport("__Internal")]
    //     private static extern void StartStreamWithArtwork_Internal(string url, string stationName, IntPtr artwork, int length);



    //     [DllImport("__Internal")]
    //     private static extern string GetMetaAsString();

    //     public static string CheckiOSMeta()
    //     {
    // #if UNITY_IOS && !UNITY_EDITOR
    //     return GetMetaAsString();
    // #else
    //         return "";
    // #endif
    //     }



    //     [DllImport("__Internal")]
    //     private static extern void UpdateNowPlaying(string title);

    //     public static void SetNowPlaying(string title)
    //     {
    // #if UNITY_IOS && !UNITY_EDITOR
    //     UpdateNowPlaying(title);
    // #endif
    //     }

}


