using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;

public static class PostProcessBuildScript
{
    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target != BuildTarget.iOS) return;

        string projPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
        PBXProject proj = new PBXProject();
        proj.ReadFromFile(projPath);

#if UNITY_2020_1_OR_NEWER
        string targetGuid = proj.GetUnityMainTargetGuid();
#else
        string targetGuid = proj.TargetGuidByName("Unity-iPhone");
#endif

        proj.AddFrameworkToProject(targetGuid, "MediaPlayer.framework", false);
        proj.AddFrameworkToProject(targetGuid, "Network.framework", false);

        // Enable Obj-C exceptions (needed for MPRemoteCommandCenter + QAVRouteChangeNotification edge cases)
        proj.AddBuildProperty(targetGuid, "GCC_ENABLE_OBJC_EXCEPTIONS", "YES");

        proj.WriteToFile(projPath);
    }
}
