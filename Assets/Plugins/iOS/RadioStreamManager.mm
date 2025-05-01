#import <AVFoundation/AVFoundation.h>
#import <MediaPlayer/MediaPlayer.h>
#import <UIKit/UIKit.h>
#import <SystemConfiguration/SystemConfiguration.h>
#import <Network/Network.h>
#import "NSObject+KVOBlock.h"


void fetchNowPlaying(NSString *urlStr);
bool IsNetworkReachable(void);
void setupRemoteCommands(void);
void setupNetworkMonitor(void);
//extern "C" void StartStream(const char* url);  // ✅ CORRECT
// extern "C" void StartStream(const char* url);



// --- State tracking ---å
typedef NS_ENUM(NSInteger, PlaybackState) {
    StateInitial,
    StateStopped,
    StateBuffering,
    StatePlaying,
    StateError,
    StateOffline
};

static PlaybackState currentState = StateInitial;
static AVPlayer *player = nil;
static AVPlayerItem *playerItem = nil;
static NSString *lastStreamUrl = nil;
static nw_path_monitor_t pathMonitor = nil;
static NSTimer *metadataTimer = nil;
static NSString *nowPlayingText = @"Ready to go...";
static UIImage *currentFavicon = nil;
static NSString *currentStationName = @"";


#pragma mark - Playback Control

/*
Who         | Does What
Unity C#    | Calls iOSRadioLauncher.FetchNowPlayingMeta(stationId) via API
Unity C#    | Updates your UI based on now_playing text
Unity C#    | Calls iOSRadioLauncher.UpdateLockscreenMeta(string title) to push text to lockscreen
iOS Obj-C   | Only accepts updated metadata and shows it
*/

void updatePlayerState(PlaybackState newState) {
    currentState = newState;

    if (metadataTimer) {
        [metadataTimer invalidate];
        metadataTimer = nil;
    }

    if (newState == StatePlaying) {
        metadataTimer = [NSTimer scheduledTimerWithTimeInterval:30.0 repeats:YES block:^(NSTimer * _Nonnull timer) {
            if (currentStationName != nil && currentStationName.length > 0) {
                // C# side will be responsible for fetching metadata now.
                NSLog(@"✅ iOS: Timer tick - C# will fetch now_playing via API.");
            }
        }];
    }

    if (newState == StateStopped || newState == StateOffline || newState == StateError) {
        [[MPNowPlayingInfoCenter defaultCenter] setNowPlayingInfo:nil];
    }
}


extern "C" void UpdateNowPlayingText(const char* text)
{
    @autoreleasepool {
        NSString *nowPlaying = [NSString stringWithUTF8String:text];
        if (nowPlaying == nil || nowPlaying.length == 0) {
            nowPlaying = @"Streaming..."; // Default text
        }
        nowPlayingText = nowPlaying;

        NSMutableDictionary *info = [NSMutableDictionary dictionary];
        [info setObject:nowPlaying forKey:MPMediaItemPropertyTitle];
        [[MPNowPlayingInfoCenter defaultCenter] setNowPlayingInfo:info];
    }
}



// Set now playing info (title + optional artwork)
extern "C" void UpdateNowPlaying(const char* title) {
    @autoreleasepool {
        NSMutableDictionary *info = [NSMutableDictionary dictionary];

        if (title) {
            NSString *titleStr = [NSString stringWithUTF8String:title];
            if (titleStr && titleStr.length > 0) {
                [info setObject:titleStr forKey:MPMediaItemPropertyTitle];
            }
        }
        [[MPNowPlayingInfoCenter defaultCenter] setNowPlayingInfo:info];
    }
}

extern "C" float GetBufferingPercent() {
    return 100.0f; // Fake full buffering — iOS AVPlayer doesn't expose buffering easily.
}

extern "C" void StartStream(const char* url)
{
    NSLog(@"✅ StartStream called");

    @autoreleasepool {
        NSString *urlStr = [NSString stringWithUTF8String:url];
        lastStreamUrl = urlStr;

        if (!IsNetworkReachable()) {
            updatePlayerState(StateOffline);
            return;
        }

        updatePlayerState(StateBuffering);

        NSURL *streamURL = [NSURL URLWithString:urlStr];
        playerItem = [AVPlayerItem playerItemWithURL:streamURL];
        player = [AVPlayer playerWithPlayerItem:playerItem];

        // Observe player item status
        [playerItem addObserverForKeyPath:@"status"
                                   options:NSKeyValueObservingOptionNew
                                   context:nil
                                  usingBlock:^(id _Nonnull object, NSDictionary<NSKeyValueChangeKey,id> * _Nonnull change) {
            AVPlayerItem *item = (AVPlayerItem *)object;
            if (item.status == AVPlayerItemStatusReadyToPlay) {
                if (currentState == StateBuffering) {
                    updatePlayerState(StatePlaying);
                }
            } else if (item.status == AVPlayerItemStatusFailed) {
                updatePlayerState(StateError);
            }
        }];

        [[AVAudioSession sharedInstance] setCategory:AVAudioSessionCategoryPlayback error:nil];
        [[AVAudioSession sharedInstance] setActive:YES error:nil];

        [[NSNotificationCenter defaultCenter] addObserverForName:AVPlayerItemFailedToPlayToEndTimeNotification
                                                           object:playerItem
                                                            queue:[NSOperationQueue mainQueue]
                                                       usingBlock:^(NSNotification * _Nonnull note) {
            updatePlayerState(StateError);
        }];

        [player play];

        // Setup remote commands and network monitoring
        setupRemoteCommands();
        setupNetworkMonitor();
    }
}


extern "C" void StartStreamWithArtwork(const char* url, const char* station, void* imageData, int length)
{
    NSLog(@"✅ StartStreamWithArtwork_Internal called");
    @autoreleasepool {
        NSData *data = [NSData dataWithBytes:imageData length:length];

        UIImage *image = [UIImage imageWithData:data];
        NSLog(@"Decoded image size: %@", NSStringFromCGSize(image.size));

        // Save for lockscreen display
        currentFavicon = image;

        NSString *urlStr = [NSString stringWithUTF8String:url];
        lastStreamUrl = urlStr;
        currentStationName = [NSString stringWithUTF8String:station];

        StartStream(url); // Reuse existing logic
    }
}

extern "C" void StartStreamWithArtwork_Internal(const char* url, const char* station, void* imageData, int length)
{
    NSLog(@"✅ StartStreamWithArtwork_Internal called");
    StartStreamWithArtwork(url, station, imageData, length);
}



extern "C" void StopStream()
{
    if (player) {
        [player pause];
        player = nil;
        playerItem = nil;
    }
    currentFavicon = nil;
    lastStreamUrl = nil;
     updatePlayerState(StateStopped);
}

extern "C" const char* GetPlaybackState()
{

static const char* state = "STOPPED"; // fallback

    switch (currentState) {
        case StateInitial:   return "INITIAL"; break;
        case StatePlaying:   return "PLAYING"; break;
        case StateBuffering: return "BUFFERING"; break;
        case StateStopped:   return "STOPPED"; break;
        case StateError:     return "ERROR"; break;
        default:             return "STOPPED"; break;
    }
    return state;
}

// Update now playing info on the lock screen
extern "C" void updateNowPlayingLockscreen(NSString *title) {
    @autoreleasepool {
        NSMutableDictionary *info = [NSMutableDictionary dictionary];
        [info setObject:title forKey:MPMediaItemPropertyTitle];

        if (currentFavicon) {
            MPMediaItemArtwork *artwork = [[MPMediaItemArtwork alloc] initWithBoundsSize:currentFavicon.size requestHandler:^UIImage * _Nonnull(CGSize size) {
                return currentFavicon;
            }];
            [info setObject:artwork forKey:MPMediaItemPropertyArtwork];
        }

        [[MPNowPlayingInfoCenter defaultCenter] setNowPlayingInfo:info];
    }
}


void setupRemoteCommands(void) {
    MPRemoteCommandCenter *remote = [MPRemoteCommandCenter sharedCommandCenter];
    [remote.playCommand setEnabled:YES];
    [remote.pauseCommand setEnabled:YES];  // Enable pause command as well

    // Handle play/pause toggle (stop and start)
    [remote.playCommand addTargetWithHandler:^MPRemoteCommandHandlerStatus(MPRemoteCommandEvent *event) {
        TogglePlayback();  // Toggle playback (stop or play)
        return MPRemoteCommandHandlerStatusSuccess;
    }];
    
    // Handle Bluetooth device disconnect (stop stream)
    [[NSNotificationCenter defaultCenter] addObserverForName:AVAudioSessionRouteChangeNotification
                                                        object:nil
                                                        queue:[NSOperationQueue mainQueue]
                                                    usingBlock:^(NSNotification * _Nonnull note) {
        NSDictionary *info = note.userInfo;
        NSUInteger rawReason = [info[AVAudioSessionRouteChangeReasonKey] unsignedIntegerValue];
        AVAudioSessionRouteChangeReason reason = (AVAudioSessionRouteChangeReason)rawReason;

        if (reason == AVAudioSessionRouteChangeReasonOldDeviceUnavailable) {
            HandleBluetoothDisconnection();  // Stop stream when BT headset is disconnected
        }
    }];
}


void setupNetworkMonitor(void)
{
    if (pathMonitor != nil) return;

    pathMonitor = nw_path_monitor_create();
    nw_path_monitor_set_update_handler(pathMonitor, ^(nw_path_t path) {
        if (nw_path_get_status(path) == nw_path_status_satisfied) {
            if (currentState == StateOffline && lastStreamUrl != nil) {
                StartStream([lastStreamUrl UTF8String]);
            }
        } else {
            updatePlayerState(StateOffline);
        }
    });

    dispatch_queue_t queue = dispatch_queue_create("NetworkMonitor", DISPATCH_QUEUE_SERIAL);
    nw_path_monitor_set_queue(pathMonitor, queue);
    nw_path_monitor_start(pathMonitor);
}

bool IsNetworkReachable(void)
{
    NSURL *url = [NSURL URLWithString:@"https://apple.com"];
    NSURLRequest *request = [NSURLRequest requestWithURL:url cachePolicy:NSURLRequestReloadIgnoringCacheData timeoutInterval:5.0];

    __block BOOL reachable = false;
    dispatch_semaphore_t sem = dispatch_semaphore_create(0);

    [[[NSURLSession sharedSession] dataTaskWithRequest:request completionHandler:^(NSData *data, NSURLResponse *res, NSError *error) {
        reachable = (error == nil);
        dispatch_semaphore_signal(sem);
    }] resume];

    dispatch_semaphore_wait(sem, dispatch_time(DISPATCH_TIME_NOW, 6 * NSEC_PER_SEC));
    return reachable;
}