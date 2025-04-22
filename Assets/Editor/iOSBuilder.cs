using UnityEditor;
using UnityEngine;
using System;
using System.Diagnostics;
using System.IO;
using Debug = UnityEngine.Debug;

public class iOSBuilder
{
    [MenuItem("Build/iOS/Reset Build Number")]
    public static void ResetBuildNumber()
    {
        PlayerSettings.bundleVersion = "1.0.0";
        PlayerSettings.iOS.buildNumber = "1";
        PlayerSettings.Android.bundleVersionCode = 1;

        Debug.Log("✅ Reset: version = 1.0.0, iOS build = 1, Android code = 1");
    }

    [MenuItem("Build/iOS/Build Xcode Project")]
    public static void BuildXcodeProject()
    {
        // ✅ Bump patch version
        VersionBumper.BumpPatch();

        string version = PlayerSettings.bundleVersion;
        string build = PlayerSettings.iOS.buildNumber;
        if (string.IsNullOrEmpty(build)) build = "1";

        string outputPath = $"Builds/iOS/RadioNation_{version}({build})";

        Directory.CreateDirectory(outputPath);

        var buildOptions = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/RadioApp.unity" },
            locationPathName = outputPath,
            target = BuildTarget.iOS,
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(buildOptions);

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"✅ iOS build completed: {outputPath}");

            EditorApplication.delayCall += () =>
            {
                string commitMsg = $"iOS Build {version}({build}): Auto-commit from Unity";
                CommitAndPush(version, build, commitMsg);

                string tag = $"ios-v{version}";
                RunGit($"tag -a {tag} -m \"iOS Release {version} ({build})\"");
                RunGit($"push origin {tag}");
                Debug.Log($"🏷️ Git tag pushed: {tag}");
            };
        }
        else
        {
            Debug.LogError("❌ iOS build failed.");
        }
    }

    private static void CommitAndPush(string version, string build, string message)
    {
        RunGit("add .");
        RunGit($"commit -m \"{message}\"");
        RunGit("push");
    }

    private static void RunGit(string args)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo("git")
        {
            Arguments = args,
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = Directory.GetCurrentDirectory()
        };

        using (Process process = Process.Start(startInfo))
        {
            process.WaitForExit();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(output)) Debug.Log(output);
            if (!string.IsNullOrEmpty(error)) Debug.LogError(error);
        }
    }
}
