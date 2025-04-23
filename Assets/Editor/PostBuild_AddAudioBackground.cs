#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;

public class PostBuild_AddAudioBackground
{
    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
        if (target != BuildTarget.iOS) return;

        // Modify Xcode project
        string projPath = PBXProject.GetPBXProjectPath(path);
        PBXProject proj = new PBXProject();
        proj.ReadFromFile(projPath);

        string targetGUID = proj.GetUnityMainTargetGuid();

        proj.AddCapability(targetGUID, PBXCapabilityType.BackgroundModes);
        // proj.AddBackgroundModes(targetGUID, new[] { "audio" });

        proj.WriteToFile(projPath);

        // Modify Info.plist
        string plistPath = Path.Combine(path, "Info.plist");
        PlistDocument plist = new PlistDocument();
        plist.ReadFromFile(plistPath);

        string bundleId = "com.wamballa.radioapp";
        proj.SetBuildProperty(targetGUID, "PRODUCT_BUNDLE_IDENTIFIER", bundleId);
        plist.root.SetString("CFBundleIdentifier", bundleId);

        PlistElementArray bgModes = plist.root["UIBackgroundModes"] as PlistElementArray;
        if (bgModes == null)
            bgModes = plist.root.CreateArray("UIBackgroundModes");

        if (!bgModes.values.Exists(v => v.AsString() == "audio"))
            bgModes.AddString("audio");

        plist.WriteToFile(plistPath);
    }
}
#endif
