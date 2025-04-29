using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

public class iOSRadioLauncher : MonoBehaviour
{
    private float playbackTime = 0;
    public static string cachedNowPlaying = "Streaming...";


    private void Start()
    {
        Debug.Log("[IOSRADIOLAUNCHER] Start: " + CheckiOSPlaybackState());
    }

    void Update()
    {
#if UNITY_IOS && !UNITY_EDITOR
            string state = iOSRadioLauncher.CheckiOSPlaybackState();

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

    public static void FetchAndUpdateMeta(string stationName)
    {

        string url = $"https://www.wamballa.com/metadata/?station={stationName}";
        Debug.Log("[iOSRadioLauncher] FetchAndUpdateMeta+ "+url);
        RadioPlayer.Instance.StartCoroutine(iOSRadioLauncher.FetchMetaCoroutine(url));

    }

    private static IEnumerator FetchMetaCoroutine(string url)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            string nowPlaying = ExtractNowPlayingFromJson(json);
            Debug.Log($"[iOSRadioLauncher] FetchMetaCoroutine: {nowPlaying} + {Time.time}");
            cachedNowPlaying = nowPlaying;  // Store it for later use
#if UNITY_IOS && !UNITY_EDITOR
            UpdateNowPlayingText(nowPlaying);  // Update iOS lock screen with the new metadata
#endif
        }
        else
        {
            Debug.LogError("Failed to fetch metadata: " + request.error);
        }
    }

    private static string ExtractNowPlayingFromJson(string json)
    {
        try
        {
            var wrapper = JsonUtility.FromJson<NowPlayingWrapper>(json);
            return wrapper?.now_playing ?? "Streaming...";
        }
        catch
        {
            return "Streaming...";
        }
    }

    public static string CheckiOSMeta()
    {
        return cachedNowPlaying;
    }

    [DllImport("__Internal")]
    private static extern void UpdateNowPlayingText(string text);

    [DllImport("__Internal")]
    private static extern void UpdateNowPlayingLockscreen(string title);

    public void UpdateLockscreenMeta(string title)
    {
#if UNITY_IOS && !UNITY_EDITOR
    if (!string.IsNullOrEmpty(title))
        UpdateNowPlayingLockscreen(title);
#endif
    }


//     public void FetchAndUpdateMeta(string stationName)
//     {
//         string url = $"https://www.wamballa.com/metadata/?station={stationName}";
//         StartCoroutine(FetchMetaCoroutine(url));
//     }

//     private IEnumerator FetchMetaCoroutine(string url)
//     {
//         UnityWebRequest request = UnityWebRequest.Get(url);
//         yield return request.SendWebRequest();

//         if (request.result == UnityWebRequest.Result.Success)
//         {
//             string json = request.downloadHandler.text;
//             string nowPlaying = ExtractNowPlayingFromJson(json);
//             cachedNowPlaying = nowPlaying;
// #if UNITY_IOS && !UNITY_EDITOR
//             UpdateNowPlayingText(nowPlaying);
// #endif
//         }
//         else
//         {
//             Debug.LogError("Failed to fetch metadata: " + request.error);
//         }
//     }



    [Serializable]
    private class NowPlayingWrapper
    {
        public string now_playing;
    }



    //     [DllImport("__Internal")]
    //     private static extern System.IntPtr GetMetaAsString();

    //     public static string CheckiOSMeta()
    //     {
    // #if UNITY_IOS && !UNITY_EDITOR
    //     IntPtr strPtr = GetMetaAsString();
    //     if (strPtr == IntPtr.Zero) return "";
    //     return Marshal.PtrToStringUTF8(strPtr);
    // #else
    //         return "";
    // #endif
    //     }



    [DllImport("__Internal")]
    private static extern void UpdateNowPlaying(string title);

    public static void SetNowPlaying(string title)
    {
#if UNITY_IOS && !UNITY_EDITOR
        UpdateNowPlaying(title);
#endif
    }

    // [DllImport("__Internal")]
    // private static extern void StartStreamWithArtwork(string url, string stationName, byte[] artwork, int length);


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


