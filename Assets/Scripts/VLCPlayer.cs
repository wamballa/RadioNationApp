//using UnityEngine;
//using System;
//using LibVLCSharp;
//using UnityEngine.UI;
//using System.Collections.Generic;
//using System.IO;
////using UnityEngine.AdaptivePerformance.Provider;

//public class VLCPlayer : MonoBehaviour
//{
//    public bool logToConsole = true; //Log function calls and LibVLC logs to Unity console

//    public static LibVLC libVLC; //The LibVLC class is mainly used for making MediaPlayer and Media objects. You should only have one LibVLC instance.
//    public MediaPlayer mediaPlayer; //MediaPlayer is the main class we use to interact with VLC
//    public RadioPlayer radioPlayer;
//    public LibVLC GetLibVLC() => libVLC;

//    // Timers
//    private float playbackTime = 0f;  // Time in seconds

//    // App logging
//    [HideInInspector] public Queue<string> recentLogs = new Queue<string>();
//    private const int maxLogs = 10;

//    #region unity
//    void Awake()
//    {

//        Log("[VLCPlayer] Awake called");
//        //Setup LibVLC
//        if (libVLC == null)
//            CreateLibVLC();

//        //Setup Media Player
//        CreateMediaPlayer();
//    }
//    void OnDestroy()
//    {
//        //Dispose of mediaPlayer, or it will stay in nemory and keep playing audio
//        DestroyMediaPlayer();
//    }


//    // Update is called once per frame
//    void Update()
//    {
//        if (radioPlayer.playerState == RadioPlayer.PlayerState.Playing)
//        {
//            playbackTime += Time.deltaTime;
//        }
//    }
//    #endregion

//    #region vlc

//    public void DirectPlay(string path)
//    {
//        Log("[VLCPlayer] Open " + path);
//        if (mediaPlayer.Media != null)
//            mediaPlayer.Media.Dispose();
//        var trimmedPath = path.Trim(new char[] { '"' });//Windows likes to copy paths with quotes but Uri does not like to open them
//        mediaPlayer.Media = new Media(new Uri(trimmedPath));
//        //mediaPlayer.Mute = true;
//        mediaPlayer.Play();
//        radioPlayer.playerState = RadioPlayer.PlayerState.Playing;

//    }

//    public void PlayStation(string path)
//    {
//        Log("[VLCPlayer] Open " + path);
//        playbackTime = 0;

//        if (mediaPlayer.Media != null)
//            mediaPlayer.Media.Dispose();

//        var trimmedPath = path.Trim(new char[] { '"' });//Windows likes to copy paths with quotes but Uri does not like to open them

//        // Add network caching option (adjust as needed)
//        mediaPlayer.Media = new Media(trimmedPath, FromType.FromLocation, ":network-caching=1000");

//        //mediaPlayer.Media = new Media(new Uri(trimmedPath));
//        mediaPlayer.Mute = true;
//        mediaPlayer.Play();


//    }

//    public void UnMute()
//    {
//        Log("[VLCPlayer] UnMute");
//        mediaPlayer.Mute = false;
//        radioPlayer.playerState = RadioPlayer.PlayerState.Playing;
//    }

//    public void Pause()
//    {
//        Log("[VLCPlayer] Pause");
//        mediaPlayer.Pause();
//        radioPlayer.playerState = (radioPlayer.playerState == RadioPlayer.PlayerState.Playing) ? RadioPlayer.PlayerState.Paused : RadioPlayer.PlayerState.Playing;

//    }

//    public void ToggleMute()
//    {
//        Log("[VLCPlayer] Pause");
//        mediaPlayer.Mute = !mediaPlayer.Mute;
//        //radioPlayer.playerState = (radioPlayer.playerState == RadioPlayer.PlayerState.Playing) ? RadioPlayer.PlayerState.Paused : RadioPlayer.PlayerState.Playing;

//    }

//    public void Stop()
//    {
//        Log("[VLCPlayer] Stop");
//        mediaPlayer?.Stop();
//        playbackTime = 0f;       // Reset the playback time
//        radioPlayer.playerState = RadioPlayer.PlayerState.Stopped;
//    }

//    public void SetVolume(int volume = 100)
//    {
//        Log("[VLCPlayer] SetVolume " + volume);
//        mediaPlayer.SetVolume(volume);
//    }

//    public bool IsPlaying
//    {
//        get
//        {
//            if (mediaPlayer == null)
//                return false;
//            return mediaPlayer.IsPlaying;
//        }
//    }
//    public float GetPlaybackTime
//    {
//        get
//        {
//            return playbackTime;
//        }
//    }

//    // Does not working for streaming
//    public long Duration
//    {
//        get
//        {
//            if (mediaPlayer == null || mediaPlayer.Media == null)
//            {
//                return 0;
//            }
//            Log("duration = " + mediaPlayer.Media.Duration);
//            return mediaPlayer.Media.Duration;
//        }
//    }
//    #endregion

//    //Private functions create and destroy VLC objects and textures
//    #region internal
//    //Create a new static LibVLC instance and dispose of the old one. You should only ever have one LibVLC instance.
//    void CreateLibVLC()
//    {
//        Log("[VLCPlayer] CreateLibVLC");
//        //Dispose of the old libVLC if necessary
//        if (libVLC != null)
//        {
//            libVLC.Dispose();
//            libVLC = null;
//        }

//        //#if UNITY_ANDROID && !UNITY_EDITOR
//        //        LibVLCSharp.Core.Initialize(); // Android: no path needed.
//        //#else
//        //        LibVLCSharp.Core.Initialize(Application.dataPath);
//        //#endif

//        Core.Initialize(Application.dataPath); //Load VLC dlls

//        libVLC = new LibVLC(enableDebugLogs: logToConsole); //You can customize LibVLC with advanced CLI options here
//                                                    //https://wiki.videolan.org/VLC_command-line_help/
//        //libVLC = new LibVLC("--verbose=2");

//        //Setup Error Logging


//        Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
//        libVLC.Log += (s, e) =>
//        {
//            //Always use try/catch in LibVLC events.
//            //LibVLC can freeze Unity if an exception goes unhandled inside an event handler.
//            try
//            {
//                string logEntry = e.FormattedLog;
//                if (recentLogs.Count >= maxLogs)
//                    recentLogs.Dequeue();

//                recentLogs.Enqueue(logEntry);

//                if (logToConsole)
//                    Log(logEntry);
//            }
//            catch (Exception ex)
//            {
//                LogError("Exception caught in libVLC.Log: \n" + ex.ToString());
//            }

//        };
//    }

//    //Create a new MediaPlayer object and dispose of the old one.
//    void CreateMediaPlayer()
//    {
//        Log("[VLCPlayer] CreateMediaPlayer");
//        if (mediaPlayer != null)
//        {
//            DestroyMediaPlayer();
//        }
//        mediaPlayer = new MediaPlayer(libVLC);
//    }

//    //Dispose of the MediaPlayer object.
//    void DestroyMediaPlayer()
//    {
//        Log("[VLCPlayer] DestroyMediaPlayer");
//        mediaPlayer?.Stop();
//        mediaPlayer?.Dispose();
//        mediaPlayer = null;
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

//    #endregion
//}