using UnityEditor;
using UnityEngine;
using System.IO;
using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;


public class AndroidBuilder
{
    [MenuItem("Build/Android/Build AAB (for Play Store)")]
    public static void BuildAAB() => Build("aab");

    [MenuItem("Build/Android/Build APK (for Dev)")]
    public static void BuildAPK() => Build("apk");

    private static void Build(string format)
    {
        // ✅ Bump patch version before build
        VersionBumper.BumpPatch();

        // Get version info
        string version = PlayerSettings.bundleVersion;
        int code = PlayerSettings.Android.bundleVersionCode;
        string fileName = $"RadioNation_{version}({code}).{format}";
        string outputPath = $"Builds/Android/{fileName}";

        // Set build type
        EditorUserBuildSettings.buildAppBundle = (format == "aab");

        // Ensure output directory exists
        Directory.CreateDirectory("Builds/Android");

        // Backup scripts, UI, scenes, fonts, prefabs
        BackupAssets(version, code);

        // Prompt to confirm git commit
        string commitMessage = null;
        int result = EditorUtility.DisplayDialogComplex(
            "Commit Build to Git?",
            $"Build {version} ({code}) completed. Commit to GitHub?",
            "Yes", "No", "Cancel"
        );

        if (result == 0)
        {
            // Fallback default commit message
            commitMessage = $"Auto-commit from Unity build: {version} ({code})";
        }

        // Build options
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/RadioApp.unity" },
            locationPathName = outputPath,
            target = BuildTarget.Android,
            options = (format == "apk") ? BuildOptions.AutoRunPlayer : BuildOptions.None
        };

        // Run build
        var report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"✅ Build succeeded: {outputPath}");

            if (!string.IsNullOrEmpty(commitMessage))
            {
                CommitAndPushToGit(version, code, commitMessage);
            }
        }
        else
        {
            Debug.LogError("❌ Build failed.");
        }
    }
    private static void BackupAssets(string version, int code)
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string backupPath = $"Builds/Backups/RadioNation_{version}({code})";

        if (Directory.Exists(backupPath)) Directory.Delete(backupPath, true);

        Directory.CreateDirectory(backupPath);

        CopyFolder("Assets/Scripts", Path.Combine(backupPath, "Scripts"));
        CopyFolder("Assets/Scenes", Path.Combine(backupPath, "Scenes"));
        CopyFolder("Assets/Prefabs", Path.Combine(backupPath, "Prefabs"));
        CopyFolder("Assets/Fonts", Path.Combine(backupPath, "Fonts"));
        CopyFolder("Assets/UI", Path.Combine(backupPath, "UI"));

        File.WriteAllText(Path.Combine(backupPath, "README.txt"),
            $"RadioNation Backup\n------------------\nVersion: {version}\nVersion Code: {code}\nDate: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n\nIncludes:\n- Scripts\n- Scenes\n- Prefabs\n- Fonts\n- UI\n\nBackup path: {backupPath}\n");

        Debug.Log($"✅ Backup completed: {backupPath}");
    }


    // ✅ Reusable folder copy method
    private static void CopyFolder(string sourceDir, string destinationDir)
    {
        if (!Directory.Exists(sourceDir))
        {
            Debug.LogWarning($"⚠️ Source directory not found: {sourceDir}");
            return;
        }

        if (!Directory.Exists(destinationDir))
            Directory.CreateDirectory(destinationDir);

        foreach (string file in Directory.GetFiles(sourceDir))
        {
            string destFile = Path.Combine(destinationDir, Path.GetFileName(file));
            File.Copy(file, destFile, true);
        }

        foreach (string subDir in Directory.GetDirectories(sourceDir))
        {
            string destSubDir = Path.Combine(destinationDir, Path.GetFileName(subDir));
            CopyFolder(subDir, destSubDir);
        }
    }

    private static void CommitAndPushToGit(string version, int code, string message)
    {
        try
        {
            RunGitCommand("add .");
            RunGitCommand($"commit -m \"Build {version}({code}): {message}\"");
            RunGitCommand("push");
            Debug.Log("✅ Git commit and push complete.");
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Git commit failed: {e.Message}");
        }
    }

    private static void RunGitCommand(string command)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo("git")
        {
            Arguments = command,
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
