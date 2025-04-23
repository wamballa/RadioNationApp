#import <AVFoundation/AVFoundation.h>

static AVPlayer *player = nil;

extern "C" void StartStream(const char* url)
{
    @autoreleasepool {
        NSString *urlStr = [NSString stringWithUTF8String:url];
        NSURL *streamURL = [NSURL URLWithString:urlStr];
        player = [[AVPlayer alloc] initWithURL:streamURL];

        [[AVAudioSession sharedInstance] setCategory:AVAudioSessionCategoryPlayback error:nil];
        [[AVAudioSession sharedInstance] setActive:YES error:nil];

        [player play];
    }
}

extern "C" void StopStream()
{
    if (player) {
        [player pause];
        player = nil;
    }
}
