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
        string frameworkTargetGuid = proj.TargetGuidByName("UnityFramework");
#else
        string frameworkTargetGuid = proj.TargetGuidByName("UnityFramework");
#endif

        proj.AddFrameworkToProject(frameworkTargetGuid, "MediaPlayer.framework", false);
        proj.AddFrameworkToProject(frameworkTargetGuid, "Network.framework", false);

        proj.WriteToFile(projPath);
    }
}
