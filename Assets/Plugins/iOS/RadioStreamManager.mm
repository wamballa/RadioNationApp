#import <AVFoundation/AVFoundation.h>
// #import <MediaPlayer/MediaPlayer.h>
// #import <UIKit/UIKit.h>
// #import <SystemConfiguration/SystemConfiguration.h>
// #import <Network/Network.h>
// #import "NSObject+KVOBlock.h"


void fetchNowPlaying(NSString *urlStr);
bool IsNetworkReachable(void);
void setupRemoteCommands(void);
void setupNetworkMonitor(void);
void UpdateNowPlayingLockscreen(NSString* title, float playbackRate);
static void SetLastErrorReason(NSString *reason);
static void SetLastConsoleLog(NSString *log);

// static AVPlayer *player = nil;
// static AVPlayerItem *playerItem = nil;

// static nw_path_monitor_t pathMonitor = nil;
static NSTimer *metadataTimer = nil;
static NSString *nowPlayingText = @"Ready to go...";
static UIImage *currentFavicon = nil;
static NSString *currentStationName = @"";
static NSString *lastErrorReason = @"No error";
static NSString *lastConsoleLog = @"No log";
static NSString *lastStreamUrl = nil;

// #pragma mark - Playback Control

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

static BOOL audioSessionSetup = NO;

extern "C" void SetupAudioSession(void) {
    NSError *error = nil;
    AVAudioSession *session = [AVAudioSession sharedInstance];

    // Use Playback for background/remote control/Bluetooth support
    BOOL ok = [session setCategory:AVAudioSessionCategoryPlayback
                    mode:AVAudioSessionModeDefault
                    options:0
                    error:&error];
    if (!ok) {
        NSLog(@"[AudioSession] Set category error: %@", error.localizedDescription);
    }
    else {
        NSLog(@"Set category SUCCESS");
    }

    ok = [session setActive:YES error:&error];
    if (!ok) {
        NSLog(@"[AudioSession] Set active error: %@", error.localizedDescription);
    }
    else {
        NSLog(@"Set active SUCCESS");
    }

}



// static void syncPlaybackStateToNowPlaying(PlaybackState state) {
//     MPNowPlayingPlaybackState playbackState;
//     switch (state) {
//         case StatePlaying:   playbackState = MPNowPlayingPlaybackStatePlaying; break;
//         case StateStopped:   playbackState = MPNowPlayingPlaybackStateStopped; break;
//         case StateBuffering: playbackState = MPNowPlayingPlaybackStateInterrupted; break;
//         case StateError:     playbackState = MPNowPlayingPlaybackStatePaused; break;
//         default:             playbackState = MPNowPlayingPlaybackStatePaused; break;
//     }
//     [MPNowPlayingInfoCenter defaultCenter].playbackState = playbackState;
// }

// void updatePlayerState(PlaybackState newState) {
//     currentState = newState;
//     syncPlaybackStateToNowPlaying(newState);

//     if (metadataTimer) {
//         [metadataTimer invalidate];
//         metadataTimer = nil;
//     }

//     if (newState == StatePlaying) {
//         NSLog(@"[updatePlayerState] StatePlaying ... currentStationName %@", currentStationName);
//         metadataTimer = [NSTimer scheduledTimerWithTimeInterval:1.0 repeats:YES block:^(NSTimer * _Nonnull timer) {
//             if (currentStationName != nil && currentStationName.length > 0) {
//                 NSString *station = [currentStationName stringByAddingPercentEncodingWithAllowedCharacters:[NSCharacterSet URLQueryAllowedCharacterSet]];
//                 // NSLog(@"[updatePlayerState] station name %@", station);
//                 NSString *urlStr = [NSString stringWithFormat:@"https://www.wamballa.com/metadata/?station=%@", station];
//                 NSURL *url = [NSURL URLWithString:urlStr];

//                 NSURLSessionDataTask *task = [[NSURLSession sharedSession] dataTaskWithURL:url completionHandler:^(NSData * _Nullable data, NSURLResponse * _Nullable response, NSError * _Nullable error) {
//                     if (error == nil && data != nil) {
//                         NSError *jsonError = nil;
//                         NSDictionary *json = [NSJSONSerialization JSONObjectWithData:data options:0 error:&jsonError];
//                         if (jsonError == nil && [json objectForKey:@"now_playing"]) {
//                             NSString *nowPlaying = json[@"now_playing"];
//                             if (nowPlaying && nowPlaying.length > 0) {
//                                 nowPlayingText = nowPlaying;
//                                 UpdateNowPlayingLockscreen(nowPlaying, 1.0f);
//                                 // UpdateNowPlayingLockscreen(nowPlaying);
//                             }
//                         }
//                     }
//                 }];
//                 [task resume];
//             }
//         }];
//     } else {
//         // Covers stopped, offline, error, initial, etc
//         UpdateNowPlayingLockscreen(nowPlayingText, 0.0f);
//         [MPNowPlayingInfoCenter defaultCenter].playbackState = MPNowPlayingPlaybackStateStopped;

//     }

//     // if (newState == StateStopped || newState == StateOffline || newState == StateError) {

//     //     // Option 1: If you want to keep metadata and only update the rate (best for your case)
//     //     NSMutableDictionary *info = [NSMutableDictionary dictionaryWithDictionary:[MPNowPlayingInfoCenter defaultCenter].nowPlayingInfo];
//     //     if (info) {
//     //         info[MPNowPlayingInfoPropertyPlaybackRate] = @0.0; // <-- Stopped/paused
//     //         [MPNowPlayingInfoCenter defaultCenter].nowPlayingInfo = info;
//     //     }
//     //     // if (newState != StateStopped) {
//     //     //     [[MPNowPlayingInfoCenter defaultCenter] setNowPlayingInfo:nil];
//     //     // }
//     // }

// }

// extern "C" void UpdateNowPlayingText(const char* text)
// {
//     @autoreleasepool {
//         NSString *nowPlaying = [NSString stringWithUTF8String:text];
//         if (nowPlaying == nil || nowPlaying.length == 0) {
//             nowPlaying = @"Streaming..."; // Default text
//         }
//         nowPlayingText = nowPlaying;

//         NSMutableDictionary *info = [NSMutableDictionary dictionary];
//         [info setObject:nowPlaying forKey:MPMediaItemPropertyTitle];
//         [[MPNowPlayingInfoCenter defaultCenter] setNowPlayingInfo:info];
//     }
// }

// void UpdateNowPlayingLockscreen(NSString* title, float playbackRate) {

//     @autoreleasepool {



//         if (title == nil || title.length == 0) return;

//         SetLastConsoleLog(@"[UpdateNowPlayingLockscreen] ");

//         // Now using NSString directly for title
//         NSMutableDictionary *info = [NSMutableDictionary dictionary];
//         [info setObject:title forKey:MPMediaItemPropertyTitle];

//         if (currentFavicon) {
//             MPMediaItemArtwork *artwork = [[MPMediaItemArtwork alloc] initWithBoundsSize:currentFavicon.size requestHandler:^UIImage * _Nonnull(CGSize size) {
//                 return currentFavicon;
//             }];
//             [info setObject:artwork forKey:MPMediaItemPropertyArtwork];
//         }

//         [info setObject:@(playbackRate) forKey:MPNowPlayingInfoPropertyPlaybackRate]; // <-- KEY LINE!

//         [[MPNowPlayingInfoCenter defaultCenter] setNowPlayingInfo:info];
//     }
// }


// // Interruption handling
// __attribute__((constructor)) static void setupInterruptionNotifications() {
//     [[NSNotificationCenter defaultCenter] addObserverForName:AVAudioSessionInterruptionNotification
//                                                       object:nil
//                                                        queue:[NSOperationQueue mainQueue]
//                                                   usingBlock:^(NSNotification * _Nonnull note) {
//         NSDictionary *info = note.userInfo;
//         NSInteger type = [info[AVAudioSessionInterruptionTypeKey] integerValue];
//         if (type == AVAudioSessionInterruptionTypeBegan) {
//             SetLastErrorReason(@"Audio interrupted (e.g. call, Siri, or other app)");
//             //lastErrorReason = @"Audio interrupted (e.g. call, Siri, or other app)";
//             if (player) [player pause];
//             updatePlayerState(StateStopped);
//         }
//     }];
// }

// extern "C" void StartStream(const char* url)
// {

//     @autoreleasepool {

//         NSString *urlStr = [NSString stringWithUTF8String:url];

//         NSLog(@"✅ StartStream called.....lastStreamUrl: %@  urlStr: %@", lastStreamUrl, urlStr);

//         // Only restart if URL is different or player is nil
//         if (player && lastStreamUrl && [lastStreamUrl isEqualToString:urlStr]) {
//             if (player.rate == 0.0) {
//                 [player play];
//                 updatePlayerState(StatePlaying);
//             }
//             return;
//         }

//         lastStreamUrl = urlStr;

//         if (!IsNetworkReachable()) {
//             updatePlayerState(StateOffline);
//             return;
//         }

//         updatePlayerState(StateBuffering);

//         // Configure the AVAudioSession for background audio playback
//         NSError *error = nil;
//         AVAudioSession *audioSession = [AVAudioSession sharedInstance];
//         [audioSession setCategory:AVAudioSessionCategoryPlayback
//               withOptions:AVAudioSessionCategoryOptionAllowBluetooth | AVAudioSessionCategoryOptionAllowBluetoothA2DP
//                     error:&error];
//         [audioSession setActive:YES error:&error];

//         if (error) {
//             NSLog(@"Error setting up audio session: %@", error.localizedDescription);
//         }

//         // AVAudioSessionRouteDescription *route = [session currentRoute];
//         // for (AVAudioSessionPortDescription *port in route.outputs) {
//         //     SetLastConsoleLog(@"Current output: %@", port.portType); // Look for BluetoothA2DPOutput, etc.
//         // }

//         NSURL *streamURL = [NSURL URLWithString:urlStr];
//         playerItem = [AVPlayerItem playerItemWithURL:streamURL];
//         player = [AVPlayer playerWithPlayerItem:playerItem];

//         // Observe player item status
//         [playerItem addObserverForKeyPath:@"status"
//                                    options:NSKeyValueObservingOptionNew
//                                    context:nil
//                                   usingBlock:^(id _Nonnull object, NSDictionary<NSKeyValueChangeKey,id> * _Nonnull change) {
//             AVPlayerItem *item = (AVPlayerItem *)object;
//             if (item.status == AVPlayerItemStatusReadyToPlay) {
//                 if (currentState == StateBuffering) {
//                     updatePlayerState(StatePlaying);
//                 }
//             } else if (item.status == AVPlayerItemStatusFailed) {
//                 NSError *error = item.error;
//                 lastErrorReason = error ? error.localizedDescription : @"Unknown error";
//                 NSLog(@"[AVPlayerItemStatusFailed] %@", lastErrorReason);
//                 updatePlayerState(StateError);
//             }
//         }];


//         [[NSNotificationCenter defaultCenter] addObserverForName:AVPlayerItemFailedToPlayToEndTimeNotification
//                                                            object:playerItem
//                                                             queue:[NSOperationQueue mainQueue]
//                                                        usingBlock:^(NSNotification * _Nonnull note) {
//             updatePlayerState(StateError);
//         }];

//         [player play];

//         // Setup remote commands and network monitoring
//         setupRemoteCommands();
//         setupNetworkMonitor();
//     }
// }

// extern "C" void StartStreamWithArtwork(const char* url, const char* station, void* imageData, int length)
// {
//     NSLog(@"✅ StartStreamWithArtwork_Internal called");
//     @autoreleasepool {
//         NSData *data = [NSData dataWithBytes:imageData length:length];

//         UIImage *image = [UIImage imageWithData:data];
//         NSLog(@"Decoded image size: %@", NSStringFromCGSize(image.size));

//         // Save for lockscreen display
//         currentFavicon = image;

//         NSString *urlStr = [NSString stringWithUTF8String:url];
//         //lastStreamUrl = urlStr;
//         NSString *stationStr = [NSString stringWithUTF8String:station];
//         currentStationName = stationStr;

//         // Extract station ID from the URL (e.g., ?station=lbc)
//         NSURLComponents *components = [NSURLComponents componentsWithString:urlStr];
//         NSString *stationID = nil;
//         for (NSURLQueryItem *item in components.queryItems) {
//             if ([item.name isEqualToString:@"station"]) {
//                 stationID = item.value;
//                 break;
//             }
//         }
//         if (stationID) {
//             currentStationName = stationID;
//         }
//         StartStream(url); // Reuse existing logic
//     }
// }

// extern "C" void StartStreamWithArtwork_Internal(const char* url, const char* station, void* imageData, int length)
// {
//     NSLog(@"✅ StartStreamWithArtwork_Internal called");
//     StartStreamWithArtwork(url, station, imageData, length);
// }



// extern "C" void StopStream()
// {
//     if (player) {
//         [player pause];
//         //         player = nil;
//         //        playerItem = nil;
//     }
//     // currentFavicon = nil;
//     // lastStreamUrl = nil;

//     lastErrorReason = @"Stopped by user";
//     updatePlayerState(StateStopped);

//     // Optionally clear lockscreen controls completely:
//     //[[MPNowPlayingInfoCenter defaultCenter] setNowPlayingInfo:nil];

//     //[MPNowPlayingInfoCenter defaultCenter].playbackState = MPNowPlayingPlaybackStateStopped;
// }



// void setupRemoteCommands(void) {
//     MPRemoteCommandCenter *remote = [MPRemoteCommandCenter sharedCommandCenter];
//     [remote.playCommand setEnabled:YES];
//     [remote.pauseCommand setEnabled:YES]; // No pause button, only play/stop

//     // Handle play command (toggle between play and stop)

//     [remote.playCommand addTargetWithHandler:^MPRemoteCommandHandlerStatus(MPRemoteCommandEvent *event) {
//         SetLastConsoleLog(@"[setupRemoteCommands] Button Pressed ");
//         if (player && player.rate == 0.0) {
//             [player play];
//             SetLastConsoleLog(@"[setupRemoteCommands] PLAY Button on BT Headset ");
//             updatePlayerState(StatePlaying);
//         }
//         return MPRemoteCommandHandlerStatusSuccess;
//     }];

//     // Handle pause command
//     [remote.pauseCommand addTargetWithHandler:^MPRemoteCommandHandlerStatus(MPRemoteCommandEvent *event) {
//         if (player && player.rate != 0.0){
//             [player pause];
//             SetLastConsoleLog(@"[setupRemoteCommands] User press Button on BT Headset to stop");
//             updatePlayerState(StateStopped);  // Update state to stopped
//         }
//         return MPRemoteCommandHandlerStatusSuccess;
//     }];

//     // Handle stop when Bluetooth is disconnected or the button is pressed
//     [[NSNotificationCenter defaultCenter] addObserverForName:AVAudioSessionRouteChangeNotification
//                                                         object:nil
//                                                         queue:[NSOperationQueue mainQueue]
//                                                     usingBlock:^(NSNotification * _Nonnull note) {
//         NSDictionary *info = note.userInfo;
//         NSUInteger rawReason = [info[AVAudioSessionRouteChangeReasonKey] unsignedIntegerValue];
//         AVAudioSessionRouteChangeReason reason = (AVAudioSessionRouteChangeReason)rawReason;

//         if (reason == AVAudioSessionRouteChangeReasonOldDeviceUnavailable) {
//             if (player) {
//                 [player pause];  // Pause playback when Bluetooth headset is disconnected
//                 SetLastErrorReason(@"Bluetooth device disconnected");
//                 // lastErrorReason = @"Bluetooth device disconnected";
//                 updatePlayerState(StateStopped);  // Update state to stopped
//             }
//         }
//     }];
// }

// void setupNetworkMonitor(void)
// {
//     if (pathMonitor != nil) return;

//     pathMonitor = nw_path_monitor_create();
//     nw_path_monitor_set_update_handler(pathMonitor, ^(nw_path_t path) {
//         if (nw_path_get_status(path) == nw_path_status_satisfied) {
//             if (currentState == StateOffline && lastStreamUrl != nil) {
//                 StartStream([lastStreamUrl UTF8String]);
//             }
//         } else {
//             updatePlayerState(StateOffline);
//         }
//     });

//     dispatch_queue_t queue = dispatch_queue_create("NetworkMonitor", DISPATCH_QUEUE_SERIAL);
//     nw_path_monitor_set_queue(pathMonitor, queue);
//     nw_path_monitor_start(pathMonitor);
// }

// bool IsNetworkReachable(void)
// {
//     NSURL *url = [NSURL URLWithString:@"https://apple.com"];
//     NSURLRequest *request = [NSURLRequest requestWithURL:url cachePolicy:NSURLRequestReloadIgnoringCacheData timeoutInterval:5.0];

//     __block BOOL reachable = false;
//     dispatch_semaphore_t sem = dispatch_semaphore_create(0);

//     [[[NSURLSession sharedSession] dataTaskWithRequest:request completionHandler:^(NSData *data, NSURLResponse *res, NSError *error) {
//         reachable = (error == nil);
//         dispatch_semaphore_signal(sem);
//     }] resume];

//     dispatch_semaphore_wait(sem, dispatch_time(DISPATCH_TIME_NOW, 6 * NSEC_PER_SEC));
//     return reachable;
// }

// Thread-safe setter for lastErrorReason
// static void SetLastErrorReason(NSString *reason) {
//     @synchronized(lastErrorReason) {
//         lastErrorReason = reason ? [reason copy] : @"Unknown";
//     }
// }

// static void SetLastConsoleLog(NSString *log) {
//     NSDateFormatter *formatter = [[NSDateFormatter alloc] init];
//     [formatter setDateFormat:@"HH:mm:ss"];
//     NSString *currentTime = [formatter stringFromDate:[NSDate date]];
//     NSString *logWithTime = [NSString stringWithFormat:@"%@ %@", currentTime, (log ?: @"No Log")];

//     @synchronized(lastConsoleLog) {
//         lastConsoleLog = [logWithTime copy];
//     }
// }


// extern "C" const char* GetLastPlaybackError() {
//     return [lastErrorReason UTF8String];
// }

// extern "C" const char* GetLastConsoleLog() {
//     return [lastConsoleLog UTF8String];
// }

// extern "C" const char* GetNowPlayingText()
// {
//     return [nowPlayingText UTF8String];
// }

// extern "C" const char* GetLastStreamUrlText()
// {
//     return [lastStreamUrl UTF8String];
// }

// extern "C" float GetBufferingPercent() {
//     return 100.0f; // Fake full buffering — iOS AVPlayer doesn't expose buffering easily.
// }

// extern "C" float GetConsoleLogFromIOS() {
//     return 100.0f; // Fake full buffering — iOS AVPlayer doesn't expose buffering easily.
// }

// extern "C" const char* GetPlaybackState()
// {
//     static const char* state = "STOPPED"; // fallback

//     switch (currentState) {
//         case StateInitial:   return "INITIAL"; break;
//         case StatePlaying:   return "PLAYING"; break;
//         case StateBuffering: return "BUFFERING"; break;
//         case StateStopped:   return "STOPPED"; break;
//         case StateError:     return "ERROR"; break;
//         default:             return "STOPPED"; break;
//     }
//     return state;
// }
    // [remote.playCommand addTargetWithHandler:^MPRemoteCommandHandlerStatus(MPRemoteCommandEvent *event) {
    //     SetLastConsoleLog(@"[setupRemoteCommands] PLAY pressed");
    //     if (player) {
    //         if (currentState == StatePlaying) {
    //             NSLog(@"✅ setupRemoteCommands Stop playing if already playing");
    //             SetLastConsoleLog(@"setupRemoteCommands Stop playing if already playing");
    //             [player pause];  // Stop playing if already playing
    //             updatePlayerState(StateStopped);  // Update state to stopped
    //             [MPNowPlayingInfoCenter defaultCenter].playbackState = MPNowPlayingPlaybackStatePaused;
    //         } else {
    //             NSLog(@"✅ setupRemoteCommands Start playing if currently stopped");
    //             SetLastConsoleLog(@"setupRemoteCommands Start playing if currently stopped");
    //             [player play];  // Start playing if currently stopped
    //             updatePlayerState(StatePlaying);  // Update state to playing
    //             [MPNowPlayingInfoCenter defaultCenter].playbackState = MPNowPlayingPlaybackStatePlaying;
    //         }
    //     }  else if (lastStreamUrl != nil && lastStreamUrl.length > 0) {
    //         NSLog(@"✅ setupRemoteCommands If player is nil (fully stopped), restart stream from last URL");
    //         SetLastConsoleLog(@"setupRemoteCommands If player is nil (fully stopped), restart stream from last URL");
    //         // If player is nil (fully stopped), restart stream from last URL
    //         StartStream([lastStreamUrl UTF8String]);
    //     }
    //     return MPRemoteCommandHandlerStatusSuccess;
    // }];