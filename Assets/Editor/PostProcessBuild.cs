#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;

public class PostProcessBuild
{
    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
        if (target != BuildTarget.iOS) return;

        string projPath = PBXProject.GetPBXProjectPath(path);
        PBXProject proj = new PBXProject();
        proj.ReadFromFile(projPath);

#if UNITY_2020_1_OR_NEWER
        string mainTarget = proj.GetUnityMainTargetGuid();
        string frameworkTarget = proj.GetUnityFrameworkTargetGuid();
#else
        string mainTarget = proj.TargetGuidByName("Unity-iPhone");
        string frameworkTarget = proj.TargetGuidByName("UnityFramework");
#endif

        // Add background audio capability
        proj.AddCapability(mainTarget, PBXCapabilityType.BackgroundModes);
        proj.AddFrameworkToProject(frameworkTarget, "MediaPlayer.framework", false);
        proj.AddFrameworkToProject(frameworkTarget, "Network.framework", false);

        // Add IronSource.framework manually
        string ironSourcePath = "Libraries/LevelPlay/Runtime/Plugins/iOS/IronSource.framework";
        if (Directory.Exists(Path.Combine(path, ironSourcePath)))
        {
            proj.AddFileToBuild(frameworkTarget, proj.AddFile(ironSourcePath, "Frameworks/" + ironSourcePath, PBXSourceTree.Source));
            proj.AddFrameworkToProject(frameworkTarget, "IronSource.framework", false);
        }

        proj.WriteToFile(projPath);

        // Update Info.plist
        string plistPath = Path.Combine(path, "Info.plist");
        PlistDocument plist = new PlistDocument();
        plist.ReadFromFile(plistPath);

        plist.root.SetString("CFBundleIdentifier", "com.wamballa.radioapp");

        PlistElementArray bgModes = plist.root["UIBackgroundModes"] as PlistElementArray;
        if (bgModes == null)
            bgModes = plist.root.CreateArray("UIBackgroundModes");

        if (!bgModes.values.Exists(v => v.AsString() == "audio"))
            bgModes.AddString("audio");

        plist.WriteToFile(plistPath);
    }
}
#endif
