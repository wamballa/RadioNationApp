#import <AVFoundation/AVFoundation.h>
#import <MediaPlayer/MediaPlayer.h>
#import <UIKit/UIKit.h>
#import <SystemConfiguration/SystemConfiguration.h>
#import <Network/Network.h>

void fetchNowPlaying(NSString *urlStr);
bool IsNetworkReachable(void);
void setupRemoteCommands(void);
void setupNetworkMonitor(void);

// --- State tracking ---
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

#pragma mark - Playback Control

void updatePlayerState(PlaybackState newState) {
    currentState = newState;

    if (newState == StatePlaying) {
        if (metadataTimer) [metadataTimer invalidate];
        metadataTimer = [NSTimer scheduledTimerWithTimeInterval:5.0 repeats:YES block:^(NSTimer * _Nonnull timer) {
            if (lastStreamUrl != nil) {
                NSString *stationId = [[NSURL URLWithString:lastStreamUrl] query];
                if (stationId) {
                    NSString *metaUrl = [NSString stringWithFormat:@"https://wamballa.com/metadata/?%@", stationId];
                    fetchNowPlaying(metaUrl);
                }
            }
        }];
    } else {
        if (metadataTimer) {
            [metadataTimer invalidate];
            metadataTimer = nil;
        }
    }

    if (newState == StateStopped || newState == StateOffline || newState == StateError) {
        [[MPNowPlayingInfoCenter defaultCenter] setNowPlayingInfo:nil];
    }
}

void fetchNowPlaying(NSString *urlStr) {
    NSURL *url = [NSURL URLWithString:urlStr];
    if (!url) return;

    [[[NSURLSession sharedSession] dataTaskWithURL:url completionHandler:^(NSData *data, NSURLResponse *res, NSError *error) {
        if (error) return;
        NSError *jsonError;
        NSDictionary *json = [NSJSONSerialization JSONObjectWithData:data options:0 error:&jsonError];
        if (jsonError) return;

        NSString *title = json[@"now_playing"] ?: @"Streaming...";
        nowPlayingText = title;

        NSMutableDictionary *info = [NSMutableDictionary dictionary];
        [info setObject:title forKey:MPMediaItemPropertyTitle];
        [[MPNowPlayingInfoCenter defaultCenter] setNowPlayingInfo:info];
    }] resume];
}

extern "C" void StartStream(const char* url)
{
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

        [[AVAudioSession sharedInstance] setCategory:AVAudioSessionCategoryPlayback error:nil];
        [[AVAudioSession sharedInstance] setActive:YES error:nil];

        [[NSNotificationCenter defaultCenter] addObserverForName:AVPlayerItemNewAccessLogEntryNotification object:playerItem queue:[NSOperationQueue mainQueue] usingBlock:^(NSNotification * _Nonnull note) {
        if (currentState == StateBuffering) {
            updatePlayerState(StatePlaying);
        }
        }];

        [[NSNotificationCenter defaultCenter] addObserverForName:AVPlayerItemFailedToPlayToEndTimeNotification object:playerItem queue:[NSOperationQueue mainQueue] usingBlock:^(NSNotification * _Nonnull note) {
            updatePlayerState(StateError);
        }];
        

        [player play];

        // Setup remote commands
        setupRemoteCommands();
        setupNetworkMonitor();
    }
}

extern "C" void StopStream()
{
    if (player) {
        [player pause];
        player = nil;
        playerItem = nil;
    }
    lastStreamUrl = nil;
     updatePlayerState(StateStopped);
}

extern "C" const char* GetPlaybackState()
{
    switch (currentState) {
        case StateInitial:   return "INITIAL";
        case StatePlaying:   return "PLAYING";
        case StateBuffering: return "BUFFERING";
        case StateStopped:   return "STOPPED";
        case StateError:     return "ERROR";
        default:             return "STOPPED";
    }
}

// Set now playing info (title + optional artwork)
extern "C" void UpdateNowPlaying(const char* title)
{
    @autoreleasepool {
        NSMutableDictionary *info = [NSMutableDictionary dictionary];

        if (title) {
            NSString *titleStr = [NSString stringWithUTF8String:title];
            if (titleStr && titleStr.length > 0) {
                [info setObject:titleStr forKey:MPMediaItemPropertyTitle];
            
        }
        [[MPNowPlayingInfoCenter defaultCenter] setNowPlayingInfo:info];
    }
}

void setupRemoteCommands(void) {
    MPRemoteCommandCenter *remote = [MPRemoteCommandCenter sharedCommandCenter];
    [remote.playCommand setEnabled:YES];
    [remote.pauseCommand setEnabled:YES];

    [remote.playCommand addTargetWithHandler:^MPRemoteCommandHandlerStatus(MPRemoteCommandEvent *event) {
        if (player) [player play];
        return MPRemoteCommandHandlerStatusSuccess;
    }];

    [remote.pauseCommand addTargetWithHandler:^MPRemoteCommandHandlerStatus(MPRemoteCommandEvent *event) {
        if (player) [player pause];
        return MPRemoteCommandHandlerStatusSuccess;
    }];

    [[NSNotificationCenter defaultCenter] addObserverForName:AVAudioSessionRouteChangeNotification
                                                        object:nil
                                                        queue:[NSOperationQueue mainQueue]
                                                    usingBlock:^(NSNotification * _Nonnull note) {
        NSDictionary *info = note.userInfo;
        NSUInteger rawReason = [info[AVAudioSessionRouteChangeReasonKey] unsignedIntegerValue];
        AVAudioSessionRouteChangeReason reason = (AVAudioSessionRouteChangeReason)rawReason;

        if (reason == AVAudioSessionRouteChangeReasonOldDeviceUnavailable) {
            if (player) {
                [player pause];
                updatePlayerState(StateStopped);
            }
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