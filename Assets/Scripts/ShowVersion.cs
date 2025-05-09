using TMPro;
using UnityEngine;

public class ShowVersion : MonoBehaviour
{
    public TMP_Text versionText;

    void Start()
    {
        // versionText.text = $"Version {Application.version} ({GetBundleVersionCode()})";
        versionText.text = $"Version {Application.version}";

    }
}
//     int GetBundleVersionCode()
//     {
// #if UNITY_ANDROID && !UNITY_EDITOR
//     using (var versionCodeClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
//     {
//         var context = versionCodeClass.GetStatic<AndroidJavaObject>("currentActivity");
//         var pm = context.Call<AndroidJavaObject>("getPackageManager");
//         var pkgName = context.Call<string>("getPackageName");
//         var pkgInfo = pm.Call<AndroidJavaObject>("getPackageInfo", pkgName, 0);
//         return pkgInfo.Get<int>("versionCode");
//     }
// #elif UNITY_IOS && !UNITY_EDITOR
//     return int.Parse(GetiOSBuildNumber());
// #elif UNITY_EDITOR
//     return UnityEditor.PlayerSettings.iOS.buildNumber != null
//         ? int.Parse(UnityEditor.PlayerSettings.iOS.buildNumber)
//         : 0;
// #else
//         return 0;
// #endif
//     }

// #if UNITY_IOS && !UNITY_EDITOR
// [System.Runtime.InteropServices.DllImport("__Internal")]
// private static extern string _getCFBundleVersion();

// string GetiOSBuildNumber()
// {
//     return _getCFBundleVersion();
// }
// #endif

// }
