using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StationButton : MonoBehaviour
{
    public string stationUUID; // Unique identifier for the station

    public int rankingNumber = 0;

    public Image faviconImage;
    public string streamingUrl;
    public string stationName;

    public Button playButton;
    public Button favoriteButton;
    public Image favoriteButtonHeart;
    public TMP_Text stationNameText;

    public TMP_Text tagsText;

    public Image faviconBGImage;

    public static event Action<string, string, string, Image> OnPlayStation;
    public static event Action<string> OnToggleFavourite;
    public bool logToConsole = true;
    public bool isOnFavouritesScreen;

    void Awake()
    {
        if (playButton != null)
        {
            playButton.onClick.AddListener(() => OnPlayStation?.Invoke(stationUUID, streamingUrl, stationName, faviconImage));
        }
        else LogError("Error: Cannot find playbutton");
        
        if (favoriteButton != null)
        {
            favoriteButton.onClick.AddListener(() =>
            {
                OnToggleFavourite?.Invoke(stationUUID);
            });
        }
        else LogError("StationButton] No favourite button found");

        Transform contentTransform = transform.parent;

        for (int i = 0; i < contentTransform.childCount; i++)
        {
            Transform child = contentTransform.GetChild(i);
            TMP_Text[] texts = child.GetComponentsInChildren<TMP_Text>(true);

            foreach (TMP_Text text in texts)
            {
                if (text.name.Contains("Ranking")) // or use tag / exact name
                {
                    text.text = (i ).ToString(); // Rank starts at 1
                }
            }
        }
    }

    public void SetStationData(string uuid, string ranking, Sprite faviconSprite, string name, string streamingURL, string faviconBGColour,string tags=null )
    {
        stationUUID = uuid;
        stationName = name;
        streamingUrl = streamingURL; // ✅ Assign the streaming URL

        // UI
        if (stationNameText != null) { stationNameText.text = name; } else Debug.LogError("!!!!!!!!!!!!!!");
        if (faviconImage != null) faviconImage.sprite = faviconSprite;
        if (tagsText!=null) tagsText.text = tags;
        Log("favicon colour " + faviconBGColour);
        // Convert and apply background color
        if (!string.IsNullOrEmpty(faviconBGColour) && ColorUtility.TryParseHtmlString(faviconBGColour, out Color bgColor))
        {
            faviconBGImage.color = bgColor;
        }
        else
        {
            Debug.LogError($"Invalid or empty color string: {faviconBGColour}");
        }
    }

    public void SetRankingNumber()
    {
        TMP_Text[] texts = transform.GetComponentsInChildren<TMP_Text>(true);

        foreach (TMP_Text text in texts)
        {
            if (text.name.Contains("Ranking")) // or use tag / exact name
            {
                text.text = "-"; // Rank starts at 1
            }
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
