using UnityEngine;
using UnityEngine.Playables;
using TMPro;
using System;
using Unity.VisualScripting;

public class AndroidRadioLauncher : MonoBehaviour
{
    public bool logToConsole = true;

    void Start()
    {

#if UNITY_ANDROID && !UNITY_EDITOR
    AudioSettings.Mobile.StopAudioOutput();
#endif



#if UNITY_ANDROID && !UNITY_EDITOR
    using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
    using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
    using (AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", activity,
        new AndroidJavaClass("com.wamballa.vlcwrapper.VLCRadioService")))
    {
        // ✅ THIS WORKS: Call startService on activity
        activity.Call<AndroidJavaObject>("startService", intent);
    }
#endif
    }


    private void Update()
    {

    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            string androidState = CheckAndroidPlaybackState();
            Log($"[OnApplicationFocus] Gained focus. Syncing playback state... {androidState}");
        }
    }

    void OnApplicationLostFocus()
    {
        string androidState = CheckAndroidPlaybackState();
        Log($"[OnApplicationLostFocus] Lost focus. Syncing playback state... {androidState}");
    }

    public static bool IsAndroidOnline()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
    using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
    using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
    using (var radioService = new AndroidJavaClass("com.wamballa.vlcwrapper.VLCRadioService"))
    {
        return radioService.CallStatic<bool>("getIsOnline");
    }
#else
        return true; // fallback for editor
#endif
    }


    public static string CheckAndroidPlaybackState()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
    using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
    {
        AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        using (AndroidJavaClass vlcService = new AndroidJavaClass("com.wamballa.vlcwrapper.VLCRadioService"))
        {
            return vlcService.CallStatic<string>("getPlayerStateAsString");
        }
    }
#else
        return "STOPPED";
#endif
    }

    public static string CheckAndroidMeta()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
    using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
    using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
    using (var radioService = new AndroidJavaClass("com.wamballa.vlcwrapper.VLCRadioService"))
    {
        return radioService.CallStatic<string>("getMetaAsString");
    }
#else
        return "UNITY_EDITOR";
#endif
    }


    public static void StartNativeVLC(string streamUrl, string stationName, Texture2D faviconTexture)
    {
        Debug.Log("[StartNativeVLC] StartNativeVLC " + streamUrl + ", " + stationName);
#if UNITY_ANDROID && !UNITY_EDITOR
    Debug.Log("StartNativeVLC triggered");
    using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
    {
        AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        Texture2D readableFavicon = GetReadableCopy(faviconTexture);
        byte[] pngData = readableFavicon.EncodeToPNG(); // ✅ This will now work
        sbyte[] signedBytes = Array.ConvertAll(pngData, b => unchecked((sbyte)b));

        using (AndroidJavaObject intent = new AndroidJavaObject(
            "android.content.Intent",
            activity,
            new AndroidJavaClass("com.wamballa.vlcwrapper.VLCRadioService")
        ))
        {
            intent.Call<AndroidJavaObject>("setAction", "ACTION_PLAY_NEW"); // ✅ Add this line
            intent.Call<AndroidJavaObject>("putExtra", "streamUrl", streamUrl);
            intent.Call<AndroidJavaObject>("putExtra", "stationName", stationName); // ✅ Pass station name
            intent.Call<AndroidJavaObject>("putExtra", "faviconBytes", signedBytes);
            activity.Call<AndroidJavaObject>("startService", intent);
        }
    }
    // 🔁 Sync state after triggering playback
    string androidState = CheckAndroidPlaybackState();
    Debug.Log($"[Unity] Android player state after launch: {androidState}");
#endif
    }


    void OnApplicationQuit()
    {
        StopRadioService();
    }

    public static void StopRadioService()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
    using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
    using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
    {
        using (var intent = new AndroidJavaObject("android.content.Intent", activity,
            new AndroidJavaClass("com.wamballa.vlcwrapper.VLCRadioService")))
        {
            intent.Call<AndroidJavaObject>("setAction", "ACTION_STOP");
            activity.Call<AndroidJavaObject>("startService", intent);
        }
    }
#endif
    }

    public static float GetAndroidBufferingPercent()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
    using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
    using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
    using (var radioService = new AndroidJavaClass("com.wamballa.vlcwrapper.VLCRadioService"))
    {
        return radioService.CallStatic<float>("getBufferingPercent");
    }
#else
        return 0f;
#endif
    }



    public static Texture2D GetReadableCopy(Texture2D original)
    {
        // Create temporary RenderTexture
        RenderTexture rt = RenderTexture.GetTemporary(
            original.width,
            original.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear
        );

        Graphics.Blit(original, rt);

        // Backup the currently active RenderTexture
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;

        // Create new readable Texture2D
        Texture2D readableTexture = new Texture2D(original.width, original.height, TextureFormat.RGBA32, false);
        readableTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        readableTexture.Apply();

        // Cleanup
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);

        return readableTexture;
    }

    void Log(object message)
    {
        if (logToConsole)
            Debug.Log("[AndroidRadioLauncher] "+message);
    }

    void LogError(object message)
    {
        if (logToConsole)
            Debug.LogError("[AndroidRadioLauncher] " + message);
    }
}
