using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public bool logToConsole = true;
    public Dictionary<string, List<StationButton>> stationButtonLookup = new Dictionary<string, List<StationButton>>();

    //public Dictionary<string, StationButton> stationButtonLookup = new Dictionary<string, StationButton>(); // Store all 

    void Start()
    {
        Initiate();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnEnable()
    {

    }

    private void OnDisable()
    {

    }

    private void Initiate()
    {
        // ✅ Capture all preloaded buttons at startup
        StationButton[] allButtons = FindObjectsByType<StationButton>(FindObjectsSortMode.None);
        //StationButton[] allButtons = FindObjectsOfType<StationButton>();
        stationButtonLookup.Clear();

        foreach (var button in allButtons)
        {
            if (!stationButtonLookup.ContainsKey(button.stationUUID))
                stationButtonLookup[button.stationUUID] = new List<StationButton>();

            stationButtonLookup[button.stationUUID].Add(button);
            Log($"✅ Registered button for: {button.stationUUID}");
        }

        // ✅ Update all unique UUIDs
        foreach (string uuid in stationButtonLookup.Keys)
        {
            UpdateFavoriteButtonColor(uuid);
        }

    }

    public void UpdateFavoriteButtonColor(string stationUUID)
    {
        if (!stationButtonLookup.TryGetValue(stationUUID, out List<StationButton> buttons))
        {
            Debug.LogError($"❌ No buttons found for UUID {stationUUID}");
            return;
        }

        bool isFavorite = FindFirstObjectByType<FavouritesManager>().IsFavorite(stationUUID);

        foreach (var button in buttons)
        {
            if (button.favoriteButton == null || button.favoriteButton.image == null || button.favoriteButtonHeart == null)
            {
                Debug.LogError($"❌ Incomplete favoriteButton setup on {stationUUID}");
                continue;
            }

            button.favoriteButtonHeart.color = isFavorite ? Color.red : Color.white;

            if (button.isOnFavouritesScreen)
            {
                button.gameObject.SetActive(isFavorite);
                button.SetRankingNumber();
            }

        }
        //UpdateAllFavoriteRankings();
    }

    public void ClearAllFavouriteButtonsColour()
    {
        foreach (string stationUUID in stationButtonLookup.Keys)
        {
            if (!stationButtonLookup.TryGetValue(stationUUID, out List<StationButton> buttons))
            {
                Debug.LogError($"❌ No buttons found for UUID {stationUUID}");
                return;
            }
            foreach (var button in buttons)
            {
                if (button.favoriteButton == null || button.favoriteButton.image == null || button.favoriteButtonHeart == null)
                {
                    Debug.LogError($"❌ Incomplete favoriteButton setup on {stationUUID}");
                    continue;
                }

                button.favoriteButtonHeart.color = Color.white;

                if (button.isOnFavouritesScreen)
                {
                    button.gameObject.SetActive(false);

                }

            }

        }
    }

    public void UpdateAllFavoriteRankings()
    {
        var favManager = FindFirstObjectByType<FavouritesManager>();
        var favorites = favManager.GetFavoritesList(); // List<string> of stationUUIDs

        int rank = 1;

        foreach (string uuid in favorites)
        {
            if (!stationButtonLookup.TryGetValue(uuid, out List<StationButton> buttons)) continue;

            foreach (var button in buttons)
            {
                if (!button.isOnFavouritesScreen) continue;

                TMP_Text[] texts = button.GetComponentsInChildren<TMP_Text>(true);
                foreach (var text in texts)
                {
                    if (text.name.ToLower().Contains("ranking"))
                    {
                        text.text = rank.ToString();
                        break;
                    }
                }
            }

            rank++;
        }
    }


    void Log(object message)
    {
        if (logToConsole)
            Debug.Log(message);
    }

    void LogError(object message)
    {
        if (logToConsole)
            Debug.LogError(message);
    }

}
