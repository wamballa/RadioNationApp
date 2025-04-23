using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using UnityEngine.Networking;


public class RadioPlayer : MonoBehaviour
{
    #region Variables
    [Header("Debug Settings")]
    public bool logToConsole = false;

    [Header("Engine Variables")]
    //public VLCPlayer vlcPlayer;
    //public MetadataHandler metadataHandler;
    public AndroidRadioLauncher androidRadioLauncher;

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

    #endregion


    void Start()
    {
        //VLC Event Handlers
        //Because many Unity functions can only be used on the main thread, they will fail in VLC event handlers
        //A simple way around this is to set flag variables which cause functions to be called on the next Update

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
#endif
        switch (currentState)
        {
            case "STOPPED":
                //Log("[RadioPlayer] State = Stopped");
                UpdateRadioPlayerDetails(
                    "",
                    currentState,
                    0,
                    "",
                    currentStation,
                    null
                    );
                ChangePlaybackImages();
                break;

            case "BUFFERING":
                //Log("[RadioPlayer]  State = Buffering" );
                bufferingPercent = AndroidRadioLauncher.GetAndroidBufferingPercent();
                UpdateRadioPlayerDetails(
                    "",
                    currentState,
                    bufferingPercent,
                    "buffering",
                    currentStation,
                    currentFaviconSprite
                    );
                ChangePlaybackImages();
                break;

            case "PLAYING":
                ChangePlaybackImages();

                UpdateRadioPlayerDetails(
                    "vlcPlayer.IsPlaying.ToString()",
                    currentState,
                    1,
                    AndroidRadioLauncher.CheckAndroidMeta(),
                    currentStation,
                    currentFaviconSprite
                    );
                break;

            case "UNAVAILABLE":
                //Log("[RadioPlayer]  State = Unavailble ");
                UpdateRadioPlayerDetails(
                    "Unavailable",
                    currentState,
                    0,
                    "This station is not available in your region or the link is broken.",
                    currentStation,
                    currentFaviconSprite
                );
                ChangePlaybackImages();
                break;

            case "OFFLINE":
                //Log("[RadioPlayer] State = Offline");
                UpdateRadioPlayerDetails(
                    "No Internet",
                    currentState,
                    0,
                    "No internet connection.",
                    currentStation,
                    currentFaviconSprite
                );

                ChangePlaybackImages();

                break;
        }
    }

    private void ChangePlaybackImages()
    {
        switch (currentState)
        {
            case "PLAYING":
                faviconImage.sprite = currentFaviconSprite;
                backgroundStoppedImage.enabled = false;
                backgroundPlayingImage.enabled = true;
                backgroundBufferingImage.enabled = false;
                bufferingIconImage.enabled = false;
                playIconImage.enabled = false;
                stopIconImage.enabled = true;
                idleIconImage.enabled = false;
                break;
            case "INITIAL":
                faviconImage.sprite = idleFaviconSprite;
                backgroundStoppedImage.enabled = true;
                backgroundPlayingImage.enabled = false;
                backgroundBufferingImage.enabled = false;
                bufferingIconImage.enabled = false;
                playIconImage.enabled = false;
                stopIconImage.enabled = false;
                idleIconImage.enabled = true;
                break;
            case "BUFFERING":
                faviconImage.sprite = currentFaviconSprite;
                backgroundStoppedImage.enabled = false;
                backgroundPlayingImage.enabled = false;
                backgroundBufferingImage.enabled = true;
                bufferingIconImage.enabled = true;
                playIconImage.enabled = false;
                stopIconImage.enabled = false;
                idleIconImage.enabled = false;
                break;
            case "STOPPED":
                faviconImage.sprite = currentFaviconSprite;
                backgroundStoppedImage.enabled = true;
                backgroundPlayingImage.enabled = false;
                backgroundBufferingImage.enabled = false;
                bufferingIconImage.enabled = false;
                playIconImage.enabled = true;
                stopIconImage.enabled = false;
                idleIconImage.enabled = false;
                break;
            case "OFFLINE":
                faviconImage.sprite = idleFaviconSprite;
                backgroundStoppedImage.enabled = true;
                backgroundPlayingImage.enabled = false;
                backgroundBufferingImage.enabled = false;
                bufferingIconImage.enabled = false;
                playIconImage.enabled = false;
                stopIconImage.enabled = false;
                idleIconImage.enabled = true;
                break;
        }

    }

    private void UpdateRadioPlayerDetails(string isPlaying, string _playerState, float buffering, string nowPlayingMeta, string name, Sprite _faviconSprite)
    {
        if (playerStateText != null) playerStateText.text = _playerState;
        if (nowPlayingMetaText != null) nowPlayingMetaText.text = nowPlayingMeta;

        if (currentStationText != null)
        {
            if (_playerState == "PLAYING")
                currentStationText.text = $"<color=red>Live: </color>{name}";
            else
                currentStationText.text = name;
        }
        //if (currentStationText != null) currentStationText.text = name;
        if (backgroundBufferingImage != null) backgroundBufferingImage.fillAmount = bufferingPercent / 100;
        if (faviconImage != null && _faviconSprite != null) faviconImage.sprite = _faviconSprite;
        if (durationText != null) durationText.text = androidRadioLauncher.GetPlaybackTime();
        if (isPlayingText != null) isPlayingText.text = isPlaying;

    }

    private void HandlePlayStopButtonPress()
    {
        switch (currentState)
        {
            case "PLAYING":
                Log("&&&&&&&&&& [RadioPlayer] STOP BUTTON PRESSED");
                ChangePlaybackImages();
                AndroidRadioLauncher.StopRadioService();
                //vlcPlayer.Stop();
                //SetPlayerState(PlayerState.Stopping);
                //playerState = PlayerState.Stopping;
                break;

            case "STOPPED":
                ChangePlaybackImages();
                if (currentStreamingURL != "")
                {
                    Log("&&&&&&&&&& [RadioPlayer] PLAY BUTTON PRESSED");

                    AndroidRadioLauncher.StartNativeVLC(currentStreamingURL, currentStation, currentFaviconSprite.texture);
                    //vlcPlayer.PlayStation(currentStreamingURL);
                }
                break;
        }

    }

    //private void ChangeRadioStationByUUID(string stationUUID, string streamURL, string name, Image _faviconImage)
    //{
    //    Log("[RadioPlayer::ChangeRadioStationByUUID] faviconImage = " + _faviconImage.sprite.name);

    //    ClearRadioPlayerDetails();

    //    currentStation = name;
    //    currentStreamingURL = streamURL;
    //    lastPlayedStationUrl = streamURL; // ✅ Save the last played station

    //    if (_faviconImage == null) Debug.LogError("Missing favicon");
    //    currentFaviconSprite = _faviconImage.sprite;
    //    if (currentFaviconSprite == null) Debug.LogError("currentFaviconImage NULL");

    //    vlcPlayer.PlayStation(streamURL);
    //    SetPlayerState(PlayerState.Stopped);
    //    //playerState = PlayerState.Stopped;

    //}

    private void ChangeNativeRadioStation(string stationUUID, string streamURL, string name, Image _faviconImage)
    {
        Log("[RadioPlayer::ChangeRadioStationByUUID] faviconImage = " + _faviconImage.sprite.name);

        ClearRadioPlayerDetails();

        currentStation = name;
        currentStreamingURL = streamURL;
        lastPlayedStationUrl = streamURL; // ✅ Save the last played station

        if (_faviconImage == null) Debug.LogError("Missing favicon");
        currentFaviconSprite = _faviconImage.sprite;
        if (currentFaviconSprite == null) Debug.LogError("currentFaviconImage NULL");

#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidRadioLauncher.StartNativeVLC(currentStreamingURL, currentStation, currentFaviconSprite.texture);
#elif UNITY_IOS && !UNITY_EDITOR
        iOSRadioLauncher.StartNativeStream(currentStreamingURL);
#endif
        //vlcPlayer.PlayStation(streamURL);
        // SetPlayerState(PlayerState.Stopped);
        //playerState = PlayerState.Stopped;

    }

    private void ClearRadioPlayerDetails()
    {
        UpdateRadioPlayerDetails("", "Stopped", 0, "", "", null);
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
