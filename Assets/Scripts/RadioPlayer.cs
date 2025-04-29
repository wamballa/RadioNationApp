using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class RadioPlayer : MonoBehaviour
{
    #region Variables
    [Header("Debug Settings")]
    public bool logToConsole = false;

    [Header("Engine Variables")]
    public iOSRadioLauncher iosRadioLauncher;


    [Header("Text")]
    public TMP_Text durationText;
    public TMP_Text isPlayingText;
    public TMP_Text playerStateText;
    public TMP_Text nowPlayingMetaText;
    public TMP_Text currentStationText;
    public TMP_Text bufferingText;

    // Private Variables
    private string currentStation;
    float bufferingPercent;
    private string currentNowPlaying;
    private string currentArtist;
    private string currentAlbum;
    private int numberOfRetries = 10;

    // Radio Player
    [Header("Play Back components")]
    public Button playStopButton;
    public Image backgroundBufferingImage;
    public Image backgroundPlayingImage;
    public Image backgroundStoppedImage;
    public Image bufferingIconImage;
    public Image playIconImage;
    public Image stopIconImage;
    public Image idleIconImage;


    // Favicon
    [Header("Favicon components")]
    public Image faviconImage;
    private Sprite currentFaviconSprite;
    private Sprite idleFaviconSprite;

    //public string streamingURL;
    [Header("Streaming URL trackers")]
    public string currentStreamingURL;
    private string lastPlayedStationUrl;

    // Private variables
    private string currentState;
    private string currentStationUUID;

    #endregion


    void Start()
    {

        currentStationText.maxVisibleLines = 2;

        idleFaviconSprite = faviconImage.sprite;

        backgroundBufferingImage.fillAmount = 0.0f;

        if (playStopButton != null)
        {
            playStopButton.onClick.AddListener(HandlePlayStopButtonPress);
        }
        else
        {
            LogError("[RADIOPLAYER] Error; no playstop button found!");
        }

        currentState = "INITIAL";

        ChangePlaybackImages();
    }

    private void OnEnable()
    {
        StationButton.OnPlayStation += ChangeNativeRadioStation;
    }

    private void OnDisable()
    {
        StationButton.OnPlayStation += ChangeNativeRadioStation;
    }

    // Update is called once per frame
    void Update()
    {
        HandleStates();
    }

    private void HandleStates()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
    currentState = AndroidRadioLauncher.CheckAndroidPlaybackState();
#elif UNITY_IOS && !UNITY_EDITOR
    currentState = iOSRadioLauncher.CheckiOSPlaybackState();
#else
        currentState = "STOPPED";
#endif

    // If the state is "PLAYING" and the metadata is not already cached, fetch it
    if (currentState == "PLAYING" && string.IsNullOrEmpty(iOSRadioLauncher.cachedNowPlaying))
    {
        // Fetch metadata from the API based on the current station
        iosRadioLauncher.FetchAndUpdateMeta(currentStationUUID); // This triggers the API call to update metadata
    }

        string meta = iosRadioLauncher.CheckiOSMeta();
        if (string.IsNullOrEmpty(meta))
            meta = "Streaming..."; // fallback if somehow empty

        var details = currentState switch
        {
            "STOPPED" => ("", currentState, 0f, "", currentStation, idleFaviconSprite),
            "BUFFERING" => ("", currentState, bufferingPercent, "Buffering...", currentStation, currentFaviconSprite),
            "PLAYING" => ("true", currentState, 1f, meta, currentStation, currentFaviconSprite),
            "UNAVAILABLE" => ("Unavailable", currentState, 0f, "This station is not available.", currentStation, currentFaviconSprite),
            "OFFLINE" => ("No Internet", currentState, 0f, "No internet connection.", currentStation, currentFaviconSprite),
            _ => ("", "STOPPED", 0f, "", currentStation, idleFaviconSprite)
        };

        UpdateRadioPlayerDetails(details.Item1, details.Item2, details.Item3, details.Item4, details.Item5, details.Item6);
        ChangePlaybackImages();

#if UNITY_IOS && !UNITY_EDITOR
if (currentState == "PLAYING" && iosRadioLauncher != null)
{
    iosRadioLauncher.UpdateLockscreenMeta(details.Item4); // details.Item4 = meta
}
#endif

    }
    // switch (currentState)
    // {
    //     case "STOPPED":
    //         //Log("[RadioPlayer] State = Stopped");
    //         UpdateRadioPlayerDetails(
    //             "",
    //             currentState,
    //             0,
    //             "",
    //             currentStation,
    //             currentFaviconSprite
    //             );
    //         ChangePlaybackImages();
    //         break;

    //     case "BUFFERING":
    //         //Log("[RadioPlayer]  State = Buffering" );
    //         UpdateRadioPlayerDetails(
    //             "",
    //             currentState,
    //             bufferingPercent,
    //             "buffering",
    //             currentStation,
    //             currentFaviconSprite
    //             );
    //         ChangePlaybackImages();
    //         break;

    //     case "PLAYING":
    //         ChangePlaybackImages();

    //         UpdateRadioPlayerDetails(
    //             "vlcPlayer.IsPlaying.ToString()",
    //             currentState,
    //             1,
    //             iOSRadioLauncher.CheckiOSMeta(),
    //             // "Meta data here",
    //             currentStation,
    //             currentFaviconSprite
    //             );
    //         break;

    //     case "UNAVAILABLE":
    //         //Log("[RadioPlayer]  State = Unavailble ");
    //         UpdateRadioPlayerDetails(
    //             "Unavailable",
    //             currentState,
    //             0,
    //             "This station is not available in your region or the link is broken.",
    //             currentStation,
    //             currentFaviconSprite
    //         );
    //         ChangePlaybackImages();
    //         break;

    //     case "OFFLINE":
    //         //Log("[RadioPlayer] State = Offline");
    //         UpdateRadioPlayerDetails(
    //             "No Internet",
    //             currentState,
    //             0,
    //             "No internet connection.",
    //             currentStation,
    //             currentFaviconSprite
    //         );

    //         ChangePlaybackImages();

    //         break;
    // }


    private void ChangePlaybackImages()
    {
        Debug.Log("[RadioPlayer] ChangePlaybackImages");

        Sprite favicon = currentFaviconSprite != null ? currentFaviconSprite : idleFaviconSprite;

        if (faviconImage != null)
            faviconImage.sprite = favicon;

        backgroundStoppedImage.enabled = false;
        backgroundPlayingImage.enabled = false;
        backgroundBufferingImage.enabled = false;
        bufferingIconImage.enabled = false;
        playIconImage.enabled = false;
        stopIconImage.enabled = false;
        idleIconImage.enabled = false;

        switch (currentState)
        {
            case "PLAYING":
                backgroundPlayingImage.enabled = true;
                stopIconImage.enabled = true;
                break;
            case "INITIAL":
            case "OFFLINE":
                faviconImage.sprite = idleFaviconSprite;
                backgroundStoppedImage.enabled = true;
                idleIconImage.enabled = true;
                break;
            case "BUFFERING":
                backgroundBufferingImage.enabled = true;
                bufferingIconImage.enabled = true;
                break;
            case "STOPPED":
                backgroundStoppedImage.enabled = true;
                playIconImage.enabled = true;
                break;
        }

    }

    private void UpdateRadioPlayerDetails(string isPlaying, string _playerState, float buffering, string nowPlayingMeta, string name, Sprite _faviconSprite)
    {
        Debug.Log("[RadioPlayer] UpdateRadioPlayerDetails");

        if (playerStateText != null) playerStateText.text = _playerState ?? "";
        if (nowPlayingMetaText != null) nowPlayingMetaText.text = nowPlayingMeta ?? "";
        if (currentStationText != null)
        {
            if (_playerState == "PLAYING")
                currentStationText.text = $"<color=red>Live: </color>{name}";
            else
                currentStationText.text = name;
        }
        if (backgroundBufferingImage != null) backgroundBufferingImage.fillAmount = bufferingPercent / 100;

        Sprite favicon = _faviconSprite != null ? _faviconSprite : idleFaviconSprite;
        if (faviconImage != null && favicon != null) faviconImage.sprite = favicon;

        if (durationText != null) durationText.text = iosRadioLauncher != null ? iosRadioLauncher.GetiOSPlaybackTime() : "";
        if (isPlayingText != null) isPlayingText.text = isPlaying ?? "";

    }

    private void HandlePlayStopButtonPress()
    {
        Debug.Log("HandlePlayStopButtonPress");
        switch (currentState)
        {
            case "PLAYING":
                Log("&&&&&&&&&& [RadioPlayer] STOP BUTTON PRESSED");
                ChangePlaybackImages();
#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidRadioLauncher.StopRadioService();
#elif UNITY_IOS && !UNITY_EDITOR
            iOSRadioLauncher.StopNativeStream();
#endif
                break;

            case "STOPPED":
                ChangePlaybackImages();
                if (currentStreamingURL != "")
                {
                    Log("&&&&&&&&&& [RadioPlayer] PLAY BUTTON PRESSED");

#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidRadioLauncher.StartNativeVLC(currentStreamingURL, currentStation, currentFaviconSprite.texture);
#elif UNITY_IOS && !UNITY_EDITOR
        iOSRadioLauncher.StartNativeStream(currentStreamingURL, currentStation, currentFaviconSprite.texture);
#endif
                }
                break;
        }

    }

    private void ChangeNativeRadioStation(string stationUUID, string streamURL, string name, Image _faviconImage)
    {
        Log("[RadioPlayer::ChangeRadioStationByUUID] faviconImage = " + _faviconImage.sprite.name);

        ClearRadioPlayerDetails();

        currentStationUUID = stationUUID;
        currentStation = name;
        currentStreamingURL = streamURL;
        lastPlayedStationUrl = streamURL; // ✅ Save the last played station

        if (_faviconImage == null) Debug.LogError("Missing favicon");
        currentFaviconSprite = _faviconImage.sprite;
        if (currentFaviconSprite == null) Debug.LogError("currentFaviconImage NULL");

#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidRadioLauncher.StartNativeVLC(currentStreamingURL, currentStation, currentFaviconSprite.texture);
#elif UNITY_IOS && !UNITY_EDITOR
        iOSRadioLauncher.StartNativeStream(currentStreamingURL, currentStation, currentFaviconSprite.texture);
#endif
    }

    private void ClearRadioPlayerDetails()
    {
        Debug.Log("[RadioPlayer] ClearRadioPlayerDetails");
        UpdateRadioPlayerDetails("", "Stopped", 0, "", "", idleFaviconSprite);
        backgroundBufferingImage.fillAmount = 0.0f;
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

    void LogWarning(object message)
    {
        if (logToConsole)
            Debug.LogWarning(message);
    }

}
