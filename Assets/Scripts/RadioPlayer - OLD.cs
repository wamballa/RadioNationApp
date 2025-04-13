//using System;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.EventSystems;
//using TMPro;
//using System.Collections;
//using UnityEngine.Networking;


//public class RadioPlayer : MonoBehaviour
//{
//    #region Variables
//    [Header("Debug Settings")]
//    public bool logToConsole = false;

//    [Header("Engine Variables")]
//    //public VLCPlayer vlcPlayer;
//    public MetadataHandler metadataHandler;
//    public AndroidRadioLauncher androidRadioLauncher;

//    // State
//    public enum PlayerState
//    {
//        Initial,
//        Stopping,
//        Stopped,
//        Playing,
//        Paused,
//        Buffering,
//        BufferingComplete,
//        Error, // Triggered when VLC fails to load/play a stream, Useful for showing a friendly “Station unavailable” message. Set this in OnEncounteredError
//        Offline, // When internet is lost (IsConnected == false).
//        Retrying, // When trying to auto-reconnect to last stream after connection returns. Could include retry countdowns, “Reconnecting...” messages
//        Unavailable // When station URL is invalid or blocked (e.g., geo-restricted), dead link, HTTP 403/404/503 or connection refused

//    }
//    [Header("Radioplayer State")]
//    public PlayerState playerState = PlayerState.Initial;

//    // GUI elements
//    [Header("Buttons")]
//    public Button playButton;
//    public Button pauseButton;
//    public Button stopButton;
//    public Button directButton;
//    public Button toggleMuteButton;

//    [Header("Text")]
//    public TMP_Text durationText;
//    public TMP_Text isPlayingText;
//    public TMP_Text playerStateText;
//    public TMP_Text nowPlayingMetaText;
//    public TMP_Text currentStationText;
//    public TMP_Text bufferingText;

//    // Private Variables
//    private string currentStation;
//    float bufferingPercent;
//    private string currentNowPlaying;
//    private string currentArtist;
//    private string currentAlbum;
//    private int numberOfRetries = 10;

//    // Radio Player
//    [Header("Play Back components")]
//    public Button playStopButton;
//    public Image bufferingImage;
//    public Image bufferingIconImage;
//    public Image playIconImage;
//    public Image stopIconImage;
//    public Image idleIconImage;


//    // Favicon
//    [Header("Favicon components")]
//    public Image faviconImage;
//    private Sprite currentFaviconSprite;
//    private Sprite idleFaviconSprite;

//    //public string streamingURL;
//    [Header("Streaming URL trackers")]
//    public string currentStreamingURL;
//    private string lastPlayedStationUrl;

//    // Private variables
//    private bool hasHandledError = false;

//    #endregion


//    void Start()
//    {
//        //VLC Event Handlers
//        //Because many Unity functions can only be used on the main thread, they will fail in VLC event handlers
//        //A simple way around this is to set flag variables which cause functions to be called on the next Update

//        //Debug.Log("[RadioPlayer] Start called");
//        //if (vlcPlayer == null) Debug.LogError("[RadioPlayer] No VLC Player");

//        currentStationText.maxVisibleLines = 2;

//        idleFaviconSprite = faviconImage.sprite;

//        //if (toggleMuteButton != null) toggleMuteButton.onClick.AddListener(() => { vlcPlayer.ToggleMute(); });

//        bufferingImage.fillAmount = 0.0f;

//        playStopButton.onClick.AddListener(HandlePlayStopButtonPress);

//        // StartCoroutine(NetworkMonitor());

//        ChangePlaybackImages();
//    }

//    private void OnEnable()
//    {
//        //vlcPlayer.mediaPlayer.Playing += OnPlaying;
//        //vlcPlayer.mediaPlayer.Buffering += OnBuffering;
//        StationButton.OnPlayStation += ChangeNativeRadioStation;
//        //StationButton.OnPlayStation += ChangeRadioStationByUUID;
//        //vlcPlayer.mediaPlayer.MediaChanged += OnMediaChanged;
//        //vlcPlayer.mediaPlayer.EncounteredError += OnEncounteredError;
//    }

//    private void OnDisable()
//    {
//        //vlcPlayer.mediaPlayer.Playing -= OnPlaying;
//        //vlcPlayer.mediaPlayer.Buffering -= OnBuffering;
//        StationButton.OnPlayStation += ChangeNativeRadioStation;
//        //StationButton.OnPlayStation -= ChangeRadioStationByUUID;
//        //vlcPlayer.mediaPlayer.MediaChanged -= OnMediaChanged;
//        //vlcPlayer.mediaPlayer.EncounteredError -= OnEncounteredError;
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        HandleStates();
//    }

//    private void OnPlaying(object sender, EventArgs e)
//    {
//        try
//        {
//            Log("[RadioPlayer] OnPlaying Triggered");
//        }
//        catch (Exception ex)
//        {
//            LogError($"[RadioPlayer] Exception in Playing event: {ex}");
//        }
//    }

//    //private void OnBuffering(object sender, MediaPlayerBufferingEventArgs e)
//    //{
//    //    try
//    //    {
//    //        bufferingPercent = e.Cache;  // e.Cache contains the buffering percentage
//    //        //print("----> buff " + bufferingPercent);
//    //        if (bufferingPercent < 100f)
//    //        {
//    //            //Debug.Log($"[VLCPlayer] Buffering... {bufferingPercent}%");
//    //            if (playerState != PlayerState.Buffering)
//    //            {
//    //                //SetPlayerState(PlayerState.Buffering);
//    //                playerState = PlayerState.Buffering;
//    //            }
//    //        }
//    //        else
//    //        {
//    //            // When buffering reaches 100%
//    //            //SetPlayerState(PlayerState.BufferingComplete);
//    //            playerState = PlayerState.BufferingComplete;

//    //            //canFindMeta = true;
//    //            Log("[VLCPlayer] Buffering 100% => Playback Ready");
//    //        }
//    //    }
//    //    catch (Exception ex)
//    //    {
//    //        LogError($"[VLCPlayer] Error during buffering: {ex.Message}");
//    //        // Optionally, handle the error more specifically or rethrow if you cannot handle it here
//    //    }
//    //}

//    // ---  ERROR STATE ---
//    private void OnEncounteredError(object sender, EventArgs e)
//    {
//        if (hasHandledError) return;

//        hasHandledError = true;

//        //foreach (var log in vlcPlayer.recentLogs)
//        //{
//        //    LogError(log);
//        //}

//        LogError("[RadioPlayer] OnEncounteredError: VLC encountered an error. Recent logs:");
//        SetPlayerState(PlayerState.Error);

//    }

//    //private void OnMediaChanged(object sender, MediaPlayerMediaChangedEventArgs e)
//    //{
//    //    try
//    //    {
//    //        if (e.Media != null)
//    //        {


//    //            //await e.Media.ParseAsync(VLCPlayer.libVLC, MediaParseOptions.ParseNetwork);
//    //            currentNowPlaying = e.Media.Meta(LibVLCSharp.MetadataType.NowPlaying);
//    //            currentArtist = e.Media.Meta(LibVLCSharp.MetadataType.Artist);
//    //            currentAlbum = e.Media.Meta(LibVLCSharp.MetadataType.Album);
//    //            metadataReady = true; // Flag to update UI in Update()
//    //            Log("Media Found!! "+currentNowPlaying);
//    //        }
//    //        else
//    //        {
//    //            Log("e.Media Empty");
//    //        }
//    //    }
//    //    catch (Exception ex)
//    //    {
//    //        LogError($"[VLCPlayer] Error retrieving media metadata: {ex}");
//    //    }
//    //}



//    // --- OFFLINE STATE ---

//    //#region ---- CHECK NETWORK STATUS ----
//    //private IEnumerator NetworkMonitor()
//    //{
//    //    while (true)
//    //    {
//    //        CheckNetworkState();
//    //        yield return new WaitForSeconds(3f); // adjust as needed
//    //    }
//    //}

//    //private void CheckNetworkState()
//    //{
//    //    // If the current internet connectivity (IsConnected) does not match
//    //    // what it was the last time we checked, then the status has changed.

//    //    bool currentlyConnected = Application.internetReachability != NetworkReachability.NotReachable;

//    //    if (!currentlyConnected && playerState != PlayerState.Offline)
//    //    {
//    //        SetPlayerState(PlayerState.Offline);
//    //        Debug.LogWarning("[RadioPlayer] Offline - Lost internet connection");
//    //    }
//    //    else if (currentlyConnected && playerState == PlayerState.Offline)
//    //    {
//    //        Debug.Log("[RadioPlayer] Connection restored - consider retrying last station");
//    //        StartCoroutine(RetryConnection(10));
//    //    }
//    //}

//    //// --- RETRYING STATE ---
//    //private IEnumerator RetryConnection(int maxAttempts)
//    //{
//    //    SetPlayerState(PlayerState.Retrying);

//    //    // 🔁 Retry ping for a few seconds
//    //    //int maxAttempts = numberOfRetries;
//    //    const float retryDelay = 2f;

//    //    for (int i = 0; i < maxAttempts; i++)
//    //    {
//    //        UnityWebRequest request = UnityWebRequest.Head("https://wamballa.com");
//    //        yield return request.SendWebRequest();

//    //        if (!request.result.Equals(UnityWebRequest.Result.ConnectionError) &&
//    //            !request.result.Equals(UnityWebRequest.Result.ProtocolError))
//    //        {
//    //            break; // ✅ DNS/connection is valid now
//    //        }

//    //        Debug.Log($"[RetryConnection] Attempt {i + 1}/{maxAttempts} failed. Retrying in {retryDelay}s...");
//    //        yield return new WaitForSeconds(retryDelay);
//    //    }

//    //    if (!string.IsNullOrEmpty(lastPlayedStationUrl))
//    //    {
//    //        yield return new WaitForSeconds(0.5f);
//    //        Log("[RadioPlayer] Retrying last station: " + lastPlayedStationUrl);
//    //        vlcPlayer.PlayStation(lastPlayedStationUrl);
//    //    }
//    //    else
//    //    {
//    //        LogError("[RadioPlayer] Retrying connection but no last played station");
//    //        SetPlayerState(PlayerState.Stopped);
//    //    }
//    //}

//    //// --- 4. UNAVAILABLE ---
//    //public void HandleInvalidStation()
//    //{
//    //    LogWarning("[RadioPlayer] Station appears unavailable");

//    //    StartCoroutine(RetryUnavailableStation(3));
//    //}

//    //private IEnumerator RetryUnavailableStation(int maxAttempts)
//    //{
//    //    if (vlcPlayer.IsPlaying)
//    //    {
//    //        Log("[RadioPlayer] Station became available again");
//    //        hasHandledError = false;
//    //        yield break;
//    //    }

//    //    SetPlayerState(PlayerState.Retrying);

//    //    for (int attempt = 1; attempt <= maxAttempts; attempt++)
//    //    {
//    //        Log($"[RadioPlayer] Retrying station... attempt {attempt}");
//    //        vlcPlayer.PlayStation(currentStreamingURL);
//    //        yield return new WaitForSeconds(3f); // wait a bit

//    //        // If it starts playing, break the retry loop
//    //        if (vlcPlayer.IsPlaying)
//    //        {
//    //            Log("[RadioPlayer] Station became available again");
//    //            yield break;
//    //        }
//    //    }

//    //    SetPlayerState(PlayerState.Unavailable);
//    //    hasHandledError = false;
//    //}

//    //#endregion ---- CHECK NETWORK STATUS ----

//    private void HandleStates()
//    {
//        switch (playerState)
//        {
//            case PlayerState.Stopping:
//                Log("[RadioPlayer] Pressed STOP!");
//                //vlcPlayer.Stop();
//                //if (!vlcPlayer.IsPlaying) SetPlayerState(PlayerState.Stopped);
//                break;

//            case PlayerState.Stopped:
//                //print("[RadioPlayer]  Stopped");
//                UpdateRadioPlayerDetails(
//                    "",
//                    playerState,
//                    0,
//                    "",
//                    "",
//                    null
//                    );

//                ChangePlaybackImages();

//                break;

//            case PlayerState.Buffering:
//                //print("Buffering % = " + bufferingPercent / 100);
//                bufferingImage.fillAmount = bufferingPercent;
//                UpdateRadioPlayerDetails(
//                    "",
//                    playerState,
//                    bufferingPercent,
//                    "buffering",
//                    currentStation,
//                    currentFaviconSprite
//                    );
//                ChangePlaybackImages();
//                break;

//            case PlayerState.BufferingComplete:
//                ChangePlaybackImages();
//                //vlcPlayer.UnMute();
//                SetPlayerState(PlayerState.Playing);
//                metadataHandler.StartFetchingMetadata();
//                break;

//            case PlayerState.Playing:
//                ChangePlaybackImages();

//                UpdateRadioPlayerDetails(
//                    "vlcPlayer.IsPlaying.ToString()",
//                    playerState,
//                    1,
//                    metadataHandler.nowPlayingMeta,
//                    currentStation,
//                    currentFaviconSprite
//                    );
//                break;

//            case PlayerState.Unavailable:
//                UpdateRadioPlayerDetails(
//                    "Unavailable",
//                    playerState,
//                    0,
//                    "This station is not available in your region or the link is broken.",
//                    currentStation,
//                    currentFaviconSprite
//                );
//                ChangePlaybackImages();
//                break;

//            case PlayerState.Error:
//                if (Application.internetReachability != NetworkReachability.NotReachable)
//                {
//                    Log("[RadioPlayer] Error with internet – retrying station");
//                    //HandleInvalidStation();
//                }
//                else
//                {
//                    Log("[RadioPlayer] Error – no internet");
//                    UpdateRadioPlayerDetails(
//                        "No Internet",
//                        playerState,
//                        0,
//                        "No internet connection.",
//                        currentStation,
//                        currentFaviconSprite
//                    );
//                    SetPlayerState(PlayerState.Offline);
//                    ChangePlaybackImages();
//                }
//                break;
//        }
//    }

//    private void HandlePlayStopButtonPress()
//    {
//        switch (playerState)
//        {
//            case PlayerState.Playing:
//                ChangePlaybackImages();
//                //vlcPlayer.Stop();
//                SetPlayerState(PlayerState.Stopping);
//                //playerState = PlayerState.Stopping;
//                break;

//            case PlayerState.Stopped:
//                ChangePlaybackImages();
//                if (currentStreamingURL != "")
//                {
//                    //vlcPlayer.PlayStation(currentStreamingURL);
//                }
//                break;
//        }

//    }

//    private void ChangePlaybackImages()
//    {
//        switch (playerState)
//        {
//            case PlayerState.Playing:
//                faviconImage.sprite = currentFaviconSprite;
//                bufferingIconImage.enabled = false;
//                playIconImage.enabled = false;
//                stopIconImage.enabled = true;
//                idleIconImage.enabled = false;
//                break;
//            case PlayerState.Initial:
//                faviconImage.sprite = idleFaviconSprite;
//                bufferingIconImage.enabled = false;
//                playIconImage.enabled = false;
//                stopIconImage.enabled = false;
//                idleIconImage.enabled = true;
//                break;
//            case PlayerState.Buffering:
//                faviconImage.sprite = currentFaviconSprite;
//                bufferingIconImage.enabled = true;
//                playIconImage.enabled = false;
//                stopIconImage.enabled = false;
//                idleIconImage.enabled = false;
//                break;
//            case PlayerState.Stopped:
//                faviconImage.sprite = currentFaviconSprite;
//                bufferingIconImage.enabled = false;
//                playIconImage.enabled = true;
//                stopIconImage.enabled = false;
//                idleIconImage.enabled = false;
//                break;
//            case PlayerState.Unavailable:
//                faviconImage.sprite = idleFaviconSprite;
//                bufferingIconImage.enabled = false;
//                playIconImage.enabled = false;
//                stopIconImage.enabled = false;
//                idleIconImage.enabled = true;
//                break;
//        }

//    }

//    //private void ChangeRadioStationByUUID(string stationUUID, string streamURL, string name, Image _faviconImage)
//    //{
//    //    Log("[RadioPlayer::ChangeRadioStationByUUID] faviconImage = " + _faviconImage.sprite.name);

//    //    ClearRadioPlayerDetails();

//    //    currentStation = name;
//    //    currentStreamingURL = streamURL;
//    //    lastPlayedStationUrl = streamURL; // ✅ Save the last played station

//    //    if (_faviconImage == null) Debug.LogError("Missing favicon");
//    //    currentFaviconSprite = _faviconImage.sprite;
//    //    if (currentFaviconSprite == null) Debug.LogError("currentFaviconImage NULL");

//    //    vlcPlayer.PlayStation(streamURL);
//    //    SetPlayerState(PlayerState.Stopped);
//    //    //playerState = PlayerState.Stopped;

//    //}

//    private void ChangeNativeRadioStation(string stationUUID, string streamURL, string name, Image _faviconImage)
//    {
//        Log("[RadioPlayer::ChangeRadioStationByUUID] faviconImage = " + _faviconImage.sprite.name);

//        ClearRadioPlayerDetails();

//        currentStation = name;
//        currentStreamingURL = streamURL;
//        lastPlayedStationUrl = streamURL; // ✅ Save the last played station

//        if (_faviconImage == null) Debug.LogError("Missing favicon");
//        currentFaviconSprite = _faviconImage.sprite;
//        if (currentFaviconSprite == null) Debug.LogError("currentFaviconImage NULL");

//        AndroidRadioLauncher.StartNativeVLC(streamURL, name, _faviconImage.sprite.texture);
//        //vlcPlayer.PlayStation(streamURL);
//        // SetPlayerState(PlayerState.Stopped);
//        //playerState = PlayerState.Stopped;

//    }

//    private void ClearRadioPlayerDetails()
//    {
//        UpdateRadioPlayerDetails("", PlayerState.Stopped, 0, "", "", null);
//        bufferingImage.fillAmount = 0.0f;
//    }

//    private void UpdateRadioPlayerDetails(string isPlaying, PlayerState _playerState, float buffering, string nowPlayingMeta, string name, Sprite _faviconSprite)
//    {
//        if (durationText != null)
//        {
//            //float timeInSeconds = vlcPlayer.GetPlaybackTime;
//            //TimeSpan timeSpan = TimeSpan.FromSeconds(timeInSeconds);
//            //durationText.text = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
//        }

//        if (playerStateText != null) playerStateText.text = _playerState.ToString();
//        //if (bufferingText != null) bufferingText.text = $"{buffering:0.##}%";
//        if (nowPlayingMetaText != null) nowPlayingMetaText.text = nowPlayingMeta;
//        if (currentStationText != null) currentStationText.text = name;
//        if (bufferingImage != null) bufferingImage.fillAmount = bufferingPercent / 100;
//        if (faviconImage != null && _faviconSprite != null) faviconImage.sprite = _faviconSprite;

//        if (isPlayingText != null) isPlayingText.text = isPlaying;

//        switch (_playerState)
//        {
//            case PlayerState.Playing:
//                break;
//            case PlayerState.Stopping:
//                break;
//            case PlayerState.Stopped:
//                break;

//        }

//    }

//    private void SetPlayerState(PlayerState newState)
//    {
//        Log("[Radio Player::SetPlayerState]" + newState);
//        playerState = newState;
//        ChangePlaybackImages();
//    }

//    void Log(object message)
//    {
//        if (logToConsole)
//            Debug.Log(message);
//    }

//    void LogError(object message)
//    {
//        if (logToConsole)
//            Debug.LogError(message);
//    }

//    void LogWarning(object message)
//    {
//        if (logToConsole)
//            Debug.LogWarning(message);
//    }

//}
