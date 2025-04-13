
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public class RadioStationList
{
    public List<RadioStationDetails> stations;
}

public class PrebuildStationButtons : MonoBehaviour
{
#if UNITY_EDITOR
    public GameObject stationButtonPrefab; // Assign this in Inspector
    public TextAsset jsonFile; // Drag and drop JSON file here
    private const string savePath = "Assets/Resources/StationButtonPrefabs/"; // ✅ Updated storage path

    public enum BatchProcessAmount
    {
        First1,
        First10,
        All
    }
    public BatchProcessAmount batchProcessAmount;

    // ✅ Manually trigger this function from the Unity Inspector
    [ContextMenu("Generate Station Prefabs")]
    private void GenerateAndSavePrefabs()
    {

        if (jsonFile == null)
        {
            Debug.LogError("❌ No JSON file assigned!");
            return;
        }

        string jsonContent = jsonFile.text;



        RadioStationList stationList = JsonUtility.FromJson<RadioStationList>(jsonContent);
        List<RadioStationDetails> stations = stationList.stations;

        if (stations == null || stations.Count == 0)
        {
            Debug.LogError("❌ No stations found in JSON!");
            return;
        }

        if (!Directory.Exists(savePath))
        {
            Debug.LogError("Error: missing Resource Path. Creating....");
            Directory.CreateDirectory(savePath);
        }

        switch (batchProcessAmount)
        {
            case BatchProcessAmount.First1:
                SaveStationButtonPrefab(stations[0]);
                break;
            case BatchProcessAmount.First10:
                // Debug for quick testing
                for (int i = 0; i < 10; i++)
                {
                    SaveStationButtonPrefab(stations[i]);
                }
                break;
            case BatchProcessAmount.All:
                foreach (var station in stations)
                {
                    SaveStationButtonPrefab(station);
                }
                break;
        }
        Debug.Log($"✅ All station buttons saved as prefabs in {savePath}");
    }

    private void SaveStationButtonPrefab(RadioStationDetails station)
    {
        if (stationButtonPrefab == null)
        {
            Debug.LogError("❌ Base prefab not assigned!");
            return;
        }

        string prefabPath = $"{savePath}{station.name}.prefab";

        // ✅ Check if it already exists
        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
        {
            Debug.Log($"⚠️ Prefab already exists: {prefabPath} (Skipping)");
            return;
        }

        // ✅ Create instance of base prefab
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(stationButtonPrefab);
        if (instance == null)
        {
            Debug.LogError("❌ Could not instantiate prefab.");
            return;
        }

        // ✅ Assign data
        var buttonScript = instance.GetComponent<StationButton>();
        if (buttonScript == null)
        {
            Debug.LogError("❌ StationButton script missing.");
            DestroyImmediate(instance);
            return;
        }

        string faviconFileName = Path.GetFileNameWithoutExtension(station.favicon);
        Sprite loadedSprite = Resources.Load<Sprite>($"Favicons/{faviconFileName}");

        if (loadedSprite == null)
        {
            Debug.LogError($"❌ Could not load favicon: {faviconFileName}");
            DestroyImmediate(instance);
            return;
        }

        buttonScript.SetStationData(
            station.name.Replace(" ", "_").ToLower(),
            station.ranking,
            loadedSprite,
            station.name,
            station.streaming_url,
            station.faviconBGColour,
            station.tags
        );

        // ✅ Save as Variant
        GameObject variant = PrefabUtility.SaveAsPrefabAssetAndConnect(instance, prefabPath, InteractionMode.UserAction);
        Debug.Log($"✅ Saved prefab variant: {prefabPath}");

        DestroyImmediate(instance);
    }
#endif

}


