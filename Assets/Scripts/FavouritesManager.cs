using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;

public class FavouritesManager : MonoBehaviour
{
    public bool logToConsole = true;
    public GameObject confirmDeleteFavouritesPanel;
    public GameObject confirmationFavouritesDeletedPopup;
    public StickyHeaderController favouritesStickyHeaderController;

    private const string FavoritesKey = "FavoriteStations"; // Key for PlayerPrefs storage
    private HashSet<string> favoriteStationUUIDs = new HashSet<string>(); // Store favorite UUIDs in memory


    private void Awake()
    {
        StationButton.OnToggleFavourite += ToggleFavorite; // ✅ Subscribe to event
        Log("[FavouritesManager] Subscribed to OnToggleFavourite");
    }
    private void Start()
    {
        LoadFavorites(); // Load favorites at startup
        //DebugPrintFavorites();
        confirmationFavouritesDeletedPopup.SetActive(false);
        confirmDeleteFavouritesPanel.SetActive(false);
    }

    private void OnDisable()
    {
        StationButton.OnToggleFavourite -= ToggleFavorite; // ✅ Clean up event listener
    }

    /// <summary>
    /// Toggles the favorite status of a station.
    /// </summary>
    public void ToggleFavorite(string stationUUID)
    {
        //Debug.Log($"🔄 ToggleFavorite called for: {stationUUID}");
        //Debug.Log($"📌 Stack trace: {new System.Diagnostics.StackTrace()}");

        Log("[FavouritesManager] Toggle Favourites "+stationUUID);

        if (favoriteStationUUIDs.Contains(stationUUID))
        {
            favoriteStationUUIDs.Remove(stationUUID);
            Log($"❌ FavoritesManager: Removed from favorites: {stationUUID}");
        }
        else
        {
            favoriteStationUUIDs.Add(stationUUID);
            Log($"⭐ FavoritesManager: Added to favorites: {stationUUID}");
        }
        //DebugPrintFavorites();
        SaveFavorites();
        FindFirstObjectByType<UIManager>()?.UpdateFavoriteButtonColor(stationUUID);
    }

    /// <summary>
    /// Checks if a station is in the favorites list.
    /// </summary>
    public bool IsFavorite(string stationUUID)
    {
        return favoriteStationUUIDs.Contains(stationUUID);
    }

    /// <summary>
    /// Saves the favorite station list to PlayerPrefs.
    /// </summary>
    private void SaveFavorites()
    {
        string json = JsonUtility.ToJson(new FavoriteData(favoriteStationUUIDs));
        PlayerPrefs.SetString(FavoritesKey, json);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Loads the favorite stations from PlayerPrefs.
    /// </summary>
    private void LoadFavorites()
    {
        if (PlayerPrefs.HasKey(FavoritesKey))
        {
            string json = PlayerPrefs.GetString(FavoritesKey);
            FavoriteData data = JsonUtility.FromJson<FavoriteData>(json);
            favoriteStationUUIDs = new HashSet<string>(data.favorites);
        }
    }

    /// <summary>
    /// Returns a list of all favorited station UUIDs.
    /// </summary>
    public List<string> GetFavoritesList()
    {
        return new List<string>(favoriteStationUUIDs);
    }

    public void OnClearFavourites()
    {
        if (!confirmDeleteFavouritesPanel.activeSelf)
        {
            confirmDeleteFavouritesPanel.SetActive(true);
        }
    }

    public void OnConfirmClearFavourites()
    {
        confirmDeleteFavouritesPanel.SetActive(false);
        FindFirstObjectByType<UIManager>()?.ClearAllFavouriteButtonsColour();
        ClearFavorites();
        favouritesStickyHeaderController.ResetView();
        StartCoroutine(DisplayFavouritesDeletedPanel());
    }

    public void OnDenyClearFavourites()
    {
        if (confirmDeleteFavouritesPanel.activeSelf) { confirmDeleteFavouritesPanel.SetActive(false); }
    }

    private IEnumerator DisplayFavouritesDeletedPanel()
    {
        confirmationFavouritesDeletedPopup.SetActive(true);
        yield return new WaitForSeconds(1);
        confirmationFavouritesDeletedPopup.SetActive(false);

    }

    public void ClearFavorites()
    {
        favoriteStationUUIDs.Clear(); // ✅ Remove all saved favorites
        PlayerPrefs.DeleteKey("FavoriteStations"); // ✅ Remove from PlayerPrefs
        PlayerPrefs.Save(); // ✅ Save changes

        Log("🗑️ All favorites cleared!");

        // ✅ Print what's left (should be empty)
        DebugPrintFavorites();
    }

    public void DebugPrintFavorites()
    {
        Log("📋 DEBUG PRINT: Favorite Stations: " + string.Join(", ", favoriteStationUUIDs));
    }


    [System.Serializable]
    private class FavoriteData
    {
        public List<string> favorites;
        public FavoriteData(HashSet<string> favorites) => this.favorites = new List<string>(favorites);
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
