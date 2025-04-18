package com.wamballa.vlcwrapper;

import android.app.Notification;
import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.app.Service;
import android.bluetooth.BluetoothDevice;
import android.content.Intent;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.media.AudioAttributes;
import android.media.AudioFocusRequest;
import android.media.AudioManager;
import android.os.Build;
import android.os.Handler;
import android.os.IBinder;

import androidx.annotation.Nullable;
import androidx.core.app.NotificationCompat;

import org.json.JSONObject;
import org.videolan.libvlc.LibVLC;
import org.videolan.libvlc.Media;
import org.videolan.libvlc.MediaPlayer;

import java.io.BufferedInputStream;
import java.io.BufferedReader;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.net.HttpURLConnection;
import java.net.URL;
import java.util.ArrayList;

import android.os.Looper;
import android.util.Log;
import android.net.Uri;
import android.app.PendingIntent;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;

import android.support.v4.media.session.MediaSessionCompat;
import android.support.v4.media.session.PlaybackStateCompat;
import android.view.KeyEvent;

public class VLCRadioService extends Service {

    private static final String CHANNEL_ID = "VLC_RADIO_CHANNEL";
    private static final int NOTIFICATION_ID = 1001;
    private static final String TAG = "VLCRadioService";
    private LibVLC libVLC;
    private MediaPlayer mediaPlayer;
    private String lastStreamUrl = null;
    private String currentStationName = "Wamballa Radio";
    private Bitmap currentFavicon = null;
    private static String nowPlayingText = "Ready to go...";
    public enum PlayerState {
        INITIAL,
        STOPPED,
        PLAYING,
        BUFFERING,
        ERROR,
        OFFLINE
    }
    private static PlayerState playerState = PlayerState.INITIAL;
    private static PlayerState lastKnownState = PlayerState.INITIAL;

    public static PlayerState getPlayerState() {
        return playerState;
    }

    private android.content.BroadcastReceiver connectivityReceiver;
    private boolean isMetadataReady = false;

    private AudioManager audioManager;
//    private AudioManager.OnAudioFocusChangeListener focusChangeListener;
    private MediaSessionCompat mediaSession;
    private static final boolean DEBUG_LOGGING_ENABLED = true;

    private AudioFocusRequest audioFocusRequest;
    private AudioAttributes audioAttributes;
    private boolean justRequestedFocus = false;
    private  AudioManager.OnAudioFocusChangeListener focusChangeListener;
    {
        // Instance initializer block to define focusChangeListener AFTER it exists
        focusChangeListener = focusChange -> {
            switch (focusChange) {
                case AudioManager.AUDIOFOCUS_LOSS:
                    logDebug("[AudioFocus] Lost permanent focus. State = " + playerState.name());

//                    if (justRequestedFocus) {
//                        logDebug("[AudioFocus] Ignoring loss — it was triggered by our own regain request.");
//                        justRequestedFocus = false;
//                        return;
//                    }

                    logDebug("[AudioFocus] Real focus loss — stopping playback.");
                    updatePlayerState(PlayerState.STOPPED);
                    if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
                        audioManager.abandonAudioFocusRequest(audioFocusRequest);
                    } else {
                        audioManager.abandonAudioFocus(focusChangeListener);
                    }
                    break;

                case AudioManager.AUDIOFOCUS_LOSS_TRANSIENT:
                    logDebug("[AudioFocus] Temporary loss. Pausing.");
                    updatePlayerState(PlayerState.STOPPED);
                    break;

                case AudioManager.AUDIOFOCUS_LOSS_TRANSIENT_CAN_DUCK:
                    logDebug("[AudioFocus] Ducking.");
                    break;

                case AudioManager.AUDIOFOCUS_GAIN:
                    logDebug("[AudioFocus] Gained focus again.");
                    break;
            }
        };

    }

    private final BroadcastReceiver becomingNoisyReceiver = new BroadcastReceiver() {
        @Override
        public void onReceive(Context context, Intent intent) {
            if (AudioManager.ACTION_AUDIO_BECOMING_NOISY.equals(intent.getAction())) {
                logDebug("[AUDIO_BECOMING_NOISY] Headphones or BT device disconnected — stopping playback");
                updatePlayerState(PlayerState.STOPPED);
            }
        }
    };

    private static float bufferingPercent = 0f;


    @Override
    public void onCreate() {
        super.onCreate();
        logDebug("[onCreate] Service created");
        ArrayList<String> options = new ArrayList<>();
        options.add("--no-drop-late-frames");
        options.add("--no-skip-frames");
        if (libVLC == null) libVLC = new LibVLC(this, options);
        if (mediaPlayer == null) mediaPlayer = new MediaPlayer(libVLC);

        audioManager = (AudioManager) getSystemService(Context.AUDIO_SERVICE);

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            audioAttributes = new AudioAttributes.Builder()
                    .setUsage(AudioAttributes.USAGE_MEDIA)
                    .setContentType(AudioAttributes.CONTENT_TYPE_MUSIC)
                    .build();

            audioFocusRequest = new AudioFocusRequest.Builder(AudioManager.AUDIOFOCUS_GAIN)
                    .setAudioAttributes(audioAttributes)
                    .setAcceptsDelayedFocusGain(true)
                    .setOnAudioFocusChangeListener(focusChangeListener)
                    .build();
        }


        // ✅ Define focusChangeListener BEFORE using it
//        focusChangeListener = focusChange -> {
//            switch (focusChange) {
//
//                case AudioManager.AUDIOFOCUS_LOSS:
//                    logDebug("[AudioFocus] Lost permanent focus. Stopping playback. "+playerState.name());
//
//                    if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
//                        int res = audioManager.requestAudioFocus(audioFocusRequest);
//                        if (res != AudioManager.AUDIOFOCUS_REQUEST_GRANTED) {
//                            logDebug("[AudioFocus] Confirmed real loss. Stopping playback.");
//                            updatePlayerState(PlayerState.STOPPED);
//                            audioManager.abandonAudioFocusRequest(audioFocusRequest);
//                        } else {
//                            logDebug("[AudioFocus] False loss — we still have focus. Ignoring.");
//                        }
//                    } else {
//                        logDebug("[AudioFocus] Legacy mode — assuming real loss.");
//                        updatePlayerState(PlayerState.STOPPED);
//                        audioManager.abandonAudioFocus(focusChangeListener);
//                    }
//                    break;
//
//
//
//                case AudioManager.AUDIOFOCUS_LOSS_TRANSIENT:
//                    logDebug("[AudioFocus] Temporary loss. Pausing.");
//                    updatePlayerState(PlayerState.STOPPED);
//                    break;
//
//
//                case AudioManager.AUDIOFOCUS_LOSS_TRANSIENT_CAN_DUCK:
//                    logDebug("[AudioFocus] Ducking.");
//                    // Optional: lower volume
//                    break;
//
//
//                case AudioManager.AUDIOFOCUS_GAIN:
//                    logDebug("[AudioFocus] Gained focus again.");
//                    break;
//            }
//        };

//        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
//
//            audioAttributes = new AudioAttributes.Builder()
//                .setUsage(AudioAttributes.USAGE_MEDIA)
//                .setContentType(AudioAttributes.CONTENT_TYPE_MUSIC)
//                .build();
//
//
//            audioFocusRequest = new AudioFocusRequest.Builder(AudioManager.AUDIOFOCUS_GAIN)
//                    .setAudioAttributes(audioAttributes)
//                    .setAcceptsDelayedFocusGain(true)
//                    .setWillPauseWhenDucked(true)
//                    .setOnAudioFocusChangeListener(focusChangeListener)
//                    .build();
//        }

        mediaPlayer.setEventListener(event -> {
            switch (event.type) {
                case MediaPlayer.Event.Playing:
                    logDebug("[onCreate] VLC Event: Playing");
                    // Don't use this to change PlayerState. It's triggered at the same time as buffering starts
                    break;

                case MediaPlayer.Event.Buffering:
                    bufferingPercent  = event.getBuffering(); // 0.0 to 100.0
                    // logDebug("[onCreate] VLC Event: Buffering " + percent + "%");
                    if (bufferingPercent  < 100) {
                        updatePlayerState(PlayerState.BUFFERING);
                    } else {
                        updatePlayerState(PlayerState.PLAYING);
                    }
                    break;

                case MediaPlayer.Event.EndReached:
                    logDebug("[onCreate] VLC Event: EndReached");
                    //updatePlayerState(PlayerState.STOPPED);
                    break;

                case MediaPlayer.Event.EncounteredError:
                    Log.e(TAG, "[onCreate] VLC Event: Error encountered");
                    updatePlayerState(PlayerState.ERROR);
                    break;

                default:
                    // logDebug("[ANDROID] Default: VLC Event: " + event.type);
                    break;
            }
        });
        // Register connectivity broadcast receiver
        connectivityReceiver = new android.content.BroadcastReceiver() {
            @Override
            public void onReceive(Context context, Intent intent) {
                new Handler(Looper.getMainLooper()).postDelayed(() -> {
                    boolean online = NetworkUtils.isOnline(context);
                    logDebug("[BroadcastReceiver] Connectivity changed: online = " + online);
                    logDebug("[BroadcastReceiver] player state = " + playerState.name());
                    if (!online) {
                        updatePlayerState(PlayerState.OFFLINE);
                        //stopPlayback();
                    } else if (online && lastStreamUrl != null ) {
                        //} else if (online && lastStreamUrl != null && lastKnownState == PlayerState.PLAYING) {
                        logDebug("[BroadcastReceiver] Back online, retrying stream");
                        startPlayback(lastStreamUrl);
                    }
                    else if (online && lastStreamUrl == null  ) {
                        logDebug("[BroadcastReceiver] Back online (no auto-reconnect)");
                        updatePlayerState(PlayerState.INITIAL); // Or define a new PlayerState.ONLINE
                    }
                }, 2000);
            }
        };


        registerReceiver(
                connectivityReceiver,
                new android.content.IntentFilter("android.net.conn.CONNECTIVITY_CHANGE")
        );
        // Check network state immediately on startup
        new Handler(Looper.getMainLooper()).post(() -> {
            boolean online = NetworkUtils.isOnline(getApplicationContext());
            logDebug("[Startup] Initial connectivity check: online = " + online);

            if (!online) {
                updatePlayerState(PlayerState.OFFLINE);
            }
        });

        // BLUETOOTH

        IntentFilter noisyFilter = new IntentFilter(AudioManager.ACTION_AUDIO_BECOMING_NOISY);
        registerReceiver(becomingNoisyReceiver, noisyFilter);


        mediaSession = new MediaSessionCompat(this, "VLCMediaSession");

        mediaSession.setCallback(new MediaSessionCompat.Callback() {
            @Override
            public boolean onMediaButtonEvent(Intent mediaButtonIntent) {
                KeyEvent event = mediaButtonIntent.getParcelableExtra(Intent.EXTRA_KEY_EVENT);
                if (event != null && event.getAction() == KeyEvent.ACTION_DOWN) {
                    logDebug("[Bluetooth] Media button ACTION_DOWN");

                    Intent toggleIntent = new Intent(getApplicationContext(), VLCRadioService.class)
                            .setAction("ACTION_TOGGLE");
                    startService(toggleIntent);
                    return true;
                } else {
                    logDebug("[Bluetooth] Media button IGNORED: " + (event != null ? event.getAction() : "null event"));
                }
                return false;
            }

            @Override
            public void onPlay() {
                logDebug("[Bluetooth] onPlay");
                Intent toggleIntent = new Intent(getApplicationContext(), VLCRadioService.class)
                        .setAction("ACTION_TOGGLE");
                startService(toggleIntent);
            }

            @Override
            public void onPause() {
                logDebug("[Bluetooth] onPause");
                Intent toggleIntent = new Intent(getApplicationContext(), VLCRadioService.class)
                        .setAction("ACTION_TOGGLE");
                startService(toggleIntent);
            }
        });

        mediaSession.setFlags(
                MediaSessionCompat.FLAG_HANDLES_MEDIA_BUTTONS |
                        MediaSessionCompat.FLAG_HANDLES_TRANSPORT_CONTROLS
        );
        mediaSession.setActive(true);
        // BLUETOOTH


    }
    private void updatePlayerState(PlayerState state)
    {
        // logDebug("[updatePlayerState] PlayerState from: "+playerState.name()+"  to "+state.name());

        playerState = state;

        switch (playerState){
            case PLAYING:
                metadataHandler.post(metadataRunnable);
                showOrUpdateNotification();
                break;
            case BUFFERING:
                nowPlayingText = "Buffering playback!";
                showOrUpdateNotification();
                break;
            case STOPPED:
                metadataHandler.removeCallbacks(metadataRunnable);
                stopPlayback();
                nowPlayingText = "Player stopped!";

                // SHOULD THIS BE NULL?
                lastStreamUrl = null;


                showOrUpdateNotification();
                break;
            case ERROR:
                metadataHandler.removeCallbacks(metadataRunnable);
                nowPlayingText = "Player Error!";
                showOrUpdateNotification();
                break;
            case OFFLINE:
                stopPlayback();
                metadataHandler.removeCallbacks(metadataRunnable);
                nowPlayingText = "Device Offline!";
                showOrUpdateNotification();
                break;
        }

        // BLUETOOTH
        int mediaState = (playerState == PlayerState.PLAYING) ?
                PlaybackStateCompat.STATE_PLAYING : PlaybackStateCompat.STATE_PAUSED;

        PlaybackStateCompat playbackState = new PlaybackStateCompat.Builder()
                .setActions(PlaybackStateCompat.ACTION_PLAY | PlaybackStateCompat.ACTION_PAUSE | PlaybackStateCompat.ACTION_PLAY_PAUSE)
                .setState(mediaState, 0, 1.0f)
                .build();

        if (mediaSession != null) {
            mediaSession.setPlaybackState(playbackState);
        }

        // BLUETOOTH

    }
    @Override
    public int onStartCommand(Intent intent, int flags, int startId) {

        if (intent != null) {

            String action = intent.getAction();

            if ("ACTION_REGAIN_FOCUS".equals(action)) {
                logDebug("[onStartCommand] Regain focus requested.");
                justRequestedFocus = true; // ✅ Set before requesting
                if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
                    audioManager.requestAudioFocus(audioFocusRequest);
                } else {
                    audioManager.requestAudioFocus(
                            focusChangeListener,
                            AudioManager.STREAM_MUSIC,
                            AudioManager.AUDIOFOCUS_GAIN
                    );
                }
                return START_STICKY; // ✅ MUST RETURN to prevent fallthrough!
            }

            logDebug("[onStartCommand] -- OnStartCommand");

            if ("ACTION_TOGGLE".equals(action)) {
                if (mediaPlayer != null && playerState == PlayerState.PLAYING) {
                    logDebug("[onStartCommand] Toggling: Stop requested ");
                    updatePlayerState(PlayerState.STOPPED); // 🔁 clean stop via state logic
                } else {
                    logDebug("[onStartCommand] Toggling: Play requested of "+lastStreamUrl);
                    if (lastStreamUrl != null) {
                        Log.w(TAG, "[AudioFocus] Trying playback of "+lastStreamUrl);
                        showOrUpdateNotification();
                        startPlayback(lastStreamUrl);
                    }
                }
                return START_STICKY;
            }




//            if ("ACTION_TOGGLE".equals(action)) {
//                //if (mediaPlayer != null && mediaPlayer.isPlaying()) {
//                if (mediaPlayer != null && playerState == PlayerState.PLAYING) {
//                    logDebug("[onStartCommand] ------------ Toggling: Stop requested");
//                    mediaPlayer.stop();
//                    updatePlayerState(PlayerState.STOPPED);
//                } else {
//                    logDebug("[onStartCommand] -------------Toggling: Play requested");
//                    if (lastStreamUrl != null) {
//                        showOrUpdateNotification();
//                        startPlayback(lastStreamUrl);
//                    }
//                }
//                return START_STICKY;
//            }

            if ("ACTION_PLAY_NEW".equals(action)) {
                logDebug("[onStartCommand] Play new station");

                String streamUrl = intent.getStringExtra("streamUrl");
                String nameFromUnity = intent.getStringExtra("stationName");
                byte[] faviconBytes = intent.getByteArrayExtra("faviconBytes");

                if (nameFromUnity != null) currentStationName = nameFromUnity;
                if (faviconBytes != null) {
                    currentFavicon = BitmapFactory.decodeByteArray(faviconBytes, 0, faviconBytes.length);
                }

                if (streamUrl != null) {
                    mediaPlayer.stop(); // 🛑 Stop old stream before starting new
                    showOrUpdateNotification();
                    startPlayback(streamUrl);
                }
                return START_STICKY;
            }

            if ("ACTION_STOP".equals(action)) {
                logDebug("[onStartCommand] ------------ ACTION_STOP received");
                updatePlayerState(PlayerState.STOPPED);
                return START_STICKY;
            }

        }
        logDebug("[onStartCommand] No known action, fallback start");
        return START_STICKY;
    }
    private void startPlayback(String streamUrl) {
        logDebug("[startPlayback] -- StartPlack "+streamUrl + " ... State = "+playerState.name());
        if (streamUrl == null || streamUrl.isEmpty()) return;

        lastStreamUrl = streamUrl; // ✅ Save for reuse
        isMetadataReady = false;
        nowPlayingText = "Buffering..."; // Or a custom message

        String stationId = Uri.parse(streamUrl).getQueryParameter("station");


        int result;
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            result = audioManager.requestAudioFocus(audioFocusRequest);
        } else {
            result = audioManager.requestAudioFocus(
                    focusChangeListener,
                    AudioManager.STREAM_MUSIC,
                    AudioManager.AUDIOFOCUS_GAIN
            );
        }

        if (result != AudioManager.AUDIOFOCUS_REQUEST_GRANTED) {
            Log.w(TAG, "[AudioFocus] Not granted. Cancelling playback.");
            return;
        }

        Media media = new Media(libVLC, Uri.parse(streamUrl));
        media.setHWDecoderEnabled(true, false);
        media.addOption(":network-caching=300");
        mediaPlayer.setMedia(media);
        media.release();
        mediaPlayer.play();
        updatePlayerState(PlayerState.BUFFERING);
    }
    private void showOrUpdateNotification() {
        // logDebug("[showOrUpdateNotification] State = " + playerState.name());

        // 🔁 1. Tap-to-open Unity app intent
        Intent openAppIntent = getPackageManager().getLaunchIntentForPackage(getPackageName());
        PendingIntent contentIntent = PendingIntent.getActivity(this, 0, openAppIntent, PendingIntent.FLAG_IMMUTABLE);

        // 🔁 2. Toggle play/stop intent
        Intent toggleIntent = new Intent(this, VLCRadioService.class).setAction("ACTION_TOGGLE");
        PendingIntent togglePendingIntent = PendingIntent.getService(this, 0, toggleIntent, PendingIntent.FLAG_IMMUTABLE);

        // 🔁 3. Decide icon + label based on current state
        boolean isActuallyPlaying = (playerState == PlayerState.PLAYING);
        //int icon = isActuallyPlaying ? com.wamballa.resources.R.drawable.stop_icon : android.R.drawable.ic_media_play;
        String label = isActuallyPlaying ? "Stop" : "Play";

        // 🔁 4. Build the notification
        Notification notification = new NotificationCompat.Builder(this, CHANNEL_ID)
                .setContentTitle(currentStationName)
                .setContentText((isActuallyPlaying ? nowPlayingText : playerState.name()))
                .setLargeIcon(currentFavicon)
                .setSmallIcon(android.R.drawable.ic_media_ff)
                //.setSmallIcon(icon)
                //.addAction(icon, label, togglePendingIntent)

                .setContentIntent(contentIntent)

                .setStyle(new androidx.media.app.NotificationCompat.MediaStyle()
                        .setMediaSession(mediaSession.getSessionToken())
                        .setShowActionsInCompactView(0)) // Show play/stop
                .setCategory(NotificationCompat.CATEGORY_TRANSPORT) // 🔥 This tells Android it's media
                .setPriority(NotificationCompat.PRIORITY_HIGH)
                .setVisibility(NotificationCompat.VISIBILITY_PUBLIC)

                // .setStyle(new androidx.media.app.NotificationCompat.MediaStyle().setShowActionsInCompactView(0))

                .setOngoing(isActuallyPlaying)

                .build();

        // 🔁 5. Ensure channel exists (Android 8+)
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            NotificationChannel channel = new NotificationChannel(
                    CHANNEL_ID,
                    "VLC Radio Playback",
                    NotificationManager.IMPORTANCE_LOW
            );
            NotificationManager manager = getSystemService(NotificationManager.class);
            if (manager != null) {
                manager.createNotificationChannel(channel);
            }
        }

        // 🔁 6. Promote to foreground if not already
        startForeground(NOTIFICATION_ID, notification);
    }


    private final Handler metadataHandler = new Handler(Looper.getMainLooper());
    private final Runnable metadataRunnable = new Runnable() {
        @Override
        public void run() {
            if (playerState == PlayerState.PLAYING && lastStreamUrl != null) {

                String stationId = Uri.parse(lastStreamUrl).getQueryParameter("station");
                //logDebug("-[metadataRunnable] State = "+playerState.name());
                fetchNowPlayingOnly(stationId); // new function below
            }
            metadataHandler.postDelayed(this, 5000); // repeat every 5s
        }
    };
    private void fetchNowPlayingOnly(String stationId) {
        new Thread(() -> {
            try {

                String metaUrl = "https://wamballa.com/metadata/?station=" + stationId;

                //logDebug("-[fetchNowPlayingOnly] State = "+playerState.name() + " "+metaUrl);

                HttpURLConnection connection = (HttpURLConnection) new URL(metaUrl).openConnection();
                connection.setConnectTimeout(3000);
                connection.setReadTimeout(3000);

                BufferedReader reader = new BufferedReader(new InputStreamReader(connection.getInputStream()));
                StringBuilder result = new StringBuilder();
                String line;
                while ((line = reader.readLine()) != null) result.append(line);

                JSONObject json = new JSONObject(result.toString());
                String newNowPlaying = json.optString("now_playing", "Playing...");

                //logDebug("-[fetchNowPlayingOnly] newNowPlaying = "+newNowPlaying);

                // Optional: Skip ads
//                if (newNowPlaying.contains("AD") || newNowPlaying.contains("Advert")) {
//                    newNowPlaying = "Ad Break";
//                }

                nowPlayingText = newNowPlaying;
                isMetadataReady = true;
                if (playerState == PlayerState.PLAYING) {
                    showOrUpdateNotification(); // Only show if stream is stable
                }


            } catch (Exception e) {
                Log.e(TAG, "NowPlaying poll failed: " + e.getMessage());
            }
        }).start();
    }
    private void stopPlayback() {
        if (mediaPlayer != null) {
            logDebug("[stopPlayback] ");
            mediaPlayer.stop();
            metadataHandler.removeCallbacks(metadataRunnable);
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
                audioManager.abandonAudioFocusRequest(audioFocusRequest);
            } else {
                audioManager.abandonAudioFocus(focusChangeListener);
            }
        }
    }

    public static String getPlayerStateAsString() {
        return playerState.name(); // Returns "PLAYING", "BUFFERING", etc.
    }
    public static String getMetaAsString() {
        return nowPlayingText; // Returns "PLAYING", "BUFFERING", etc.
    }

    public static boolean getIsOnline() {
        return playerState != PlayerState.OFFLINE;
    }

    public static float getBufferingPercent() {
        return bufferingPercent;
    }

    @Override
    public void onDestroy() {
        logDebug("Service destroyed");
        stopPlayback();
        if (mediaPlayer != null) mediaPlayer.release();
        if (libVLC != null) libVLC.release();
        if (connectivityReceiver != null) {
            unregisterReceiver(connectivityReceiver);
        }
        unregisterReceiver(becomingNoisyReceiver);

        super.onDestroy();
    }

    private void logDebug(String msg) {
        if (DEBUG_LOGGING_ENABLED) Log.d(TAG, msg);
    }

    @Nullable
    @Override
    public IBinder onBind(Intent intent) {
        return null;
    }
}
