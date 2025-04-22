using TMPro;
using UnityEngine;

public class ShowVersion : MonoBehaviour
{
    public TMP_Text versionText;

    void Start()
    {
        versionText.text = $"Version {Application.version} ({GetBundleVersionCode()})";
    }

    int GetBundleVersionCode()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
    using (var versionCodeClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
    {
        var context = versionCodeClass.GetStatic<AndroidJavaObject>("currentActivity");
        var pm = context.Call<AndroidJavaObject>("getPackageManager");
        var pkgName = context.Call<string>("getPackageName");
        var pkgInfo = pm.Call<AndroidJavaObject>("getPackageInfo", pkgName, 0);
        return pkgInfo.Get<int>("versionCode");
    }
#elif UNITY_EDITOR
    return UnityEditor.PlayerSettings.Android.bundleVersionCode; // Editor fallback
#else
    return 0;
#endif
    }
}
