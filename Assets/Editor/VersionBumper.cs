using System.IO;
using UnityEditor;
using UnityEngine;

public class VersionBumper
{
    [MenuItem("Build/Version/Current Version", false, 0)]
    public static void ShowCurrentVersion()
    {
        string version = PlayerSettings.bundleVersion;
        int versionCode = PlayerSettings.Android.bundleVersionCode;
        Debug.Log($"📦 Current Version: {version} ({versionCode})");
        EditorUtility.DisplayDialog("Current Version", $"Version: {version}\nVersion Code: {versionCode}", "OK");
    }

    [MenuItem("Build/Version/Bump Patch")]
    public static void BumpPatch()
    {
        BumpVersion(VersionComponent.Patch);
    }

    [MenuItem("Build/Version/Bump Minor")]
    public static void BumpMinor()
    {
        BumpVersion(VersionComponent.Minor);
    }

    [MenuItem("Build/Version/Bump Major")]
    public static void BumpMajor()
    {
        BumpVersion(VersionComponent.Major);
    }

    [MenuItem("Build/Version/Revert to Last Version")]
    public static void RevertVersion()
    {
        string backupPath = "Assets/Editor/version_backup.txt";
        if (!File.Exists(backupPath))
        {
            Debug.LogWarning("No backup found. Cannot revert.");
            return;
        }

        string[] data = File.ReadAllText(backupPath).Split('|');
        if (data.Length != 2) return;

        PlayerSettings.bundleVersion = data[0];
        PlayerSettings.Android.bundleVersionCode = int.Parse(data[1]);

        Debug.Log($"⏪ Reverted to version {data[0]}, versionCode: {data[1]}");
    }

    enum VersionComponent { Major, Minor, Patch }

    static void BumpVersion(VersionComponent component)
    {
        string version = PlayerSettings.bundleVersion;
        string[] parts = version.Split('.');
        if (parts.Length != 3) parts = new string[] { "1", "0", "0" };

        int major = int.Parse(parts[0]);
        int minor = int.Parse(parts[1]);
        int patch = int.Parse(parts[2]);

        // ✅ Save current version before bumping
        string backupPath = "Assets/Editor/version_backup.txt";
        File.WriteAllText(backupPath, $"{version}|{PlayerSettings.Android.bundleVersionCode}");

        switch (component)
        {
            case VersionComponent.Major:
                major++;
                minor = 0;
                patch = 0;
                break;
            case VersionComponent.Minor:
                minor++;
                patch = 0;
                break;
            case VersionComponent.Patch:
                patch++;
                break;
        }

        string newVersion = $"{major}.{minor}.{patch}";
        PlayerSettings.bundleVersion = newVersion;
        PlayerSettings.Android.bundleVersionCode++;

        Debug.Log($"✅ Updated to version {newVersion}, versionCode: {PlayerSettings.Android.bundleVersionCode}");
    }
}
