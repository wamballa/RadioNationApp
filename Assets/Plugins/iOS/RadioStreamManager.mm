#import <AVFoundation/AVFoundation.h>
#import <MediaPlayer/MediaPlayer.h>
#import <UIKit/UIKit.h>
#import <SystemConfiguration/SystemConfiguration.h>
#import <Network/Network.h>
#import "NSObject+KVOBlock.h"

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
static NSString *nowPlayingText = @"Ready to go...";
static UIImage *currentFavicon = nil;
static NSString *currentStationName = @"";

extern "C" void updatePlayerState(PlaybackState newState) {
    currentState = newState;

    if (newState == StateStopped || newState == StateOffline || newState == StateError) {
        [[MPNowPlayingInfoCenter defaultCenter] setNowPlayingInfo:nil];
    }
}

// Update now playing info on the lock screen
extern "C" void updateNowPlayingLockscreen(NSString *title);

// Stop stream
extern "C" void StopStream() {
    if (player) {
        [player pause];  // Pause if it's currently playing
        [player replaceCurrentItemWithPlayerItem:nil]; // Stop the stream completely
        updatePlayerState(StateStopped);  // Set state to stopped
    }
}

// Start stream
extern "C" void StartStream(const char* url) {
    NSLog(@"✅ StartStream called");

    @autoreleasepool {
        NSString *urlStr = [NSString stringWithUTF8String:url];
        lastStreamUrl = urlStr;  // Save for future play
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

        [player play];  // Start playback

        updatePlayerState(StateBuffering); // Indicate that buffering is in progress

        // Update lock screen metadata
        updateNowPlayingLockscreen(nowPlayingText);
    }
}

// Handle play/pause button (toggle between play and stop)
extern "C" void TogglePlayback() {
    if (player) {
        if (currentState == StatePlaying) {
            [player pause];  // Stop playing
            [player replaceCurrentItemWithPlayerItem:nil];  // Clear the stream
            updatePlayerState(StateStopped);  // Set state to stopped
            updateNowPlayingLockscreen(@"Stream Stopped");  // Update lock screen text
        } else {
            if (lastStreamUrl != nil) {
                StartStream([lastStreamUrl UTF8String]);  // Restart the stream from the last URL
                updatePlayerState(StatePlaying);  // Set state to playing
                updateNowPlayingLockscreen(nowPlayingText);  // Update lock screen with current title
            }
        }
    }
}

// Handle Bluetooth headset disconnection
extern "C" void HandleBluetoothDisconnection() {
    if (player) {
        [player pause];  // Pause playback when Bluetooth headset is disconnected
        [player replaceCurrentItemWithPlayerItem:nil];  // Stop the stream completely
        updatePlayerState(StateStopped);  // Set state to stopped
        updateNowPlayingLockscreen(@"Disconnected");  // Update lock screen text
    }
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

// Set up remote commands (Play/Pause button handling)
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
